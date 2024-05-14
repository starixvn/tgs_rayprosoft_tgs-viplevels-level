using System.Net.Mime;
using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.AspNetCore.Mvc;
using NATS.Client.Core;
using OpenTelemetry.Metrics;

using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.MessageQueue.Nats.Serializers;
using Tgs.VipLevels.Level;
using Tgs.VipLevels.Level.Common;
using Tgs.VipLevels.Level.Messages;

var filterInstrumentations = new[] { "html", "ico", "png", "jpg" };

var builder = WebApplication.CreateBuilder(args);

builder.Logging
	.AddOpenTelemetry(options =>
	{
		options.IncludeFormattedMessage = true;
		options.ParseStateValues = true;
		_ = options.AddFluentdForwardExporter(fluentOptions =>
		{
			var nodeIp = builder.Configuration.GetValue<string>("NODE_IP")!;
			fluentOptions.Host = nodeIp;
			fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
			fluentOptions.UseMessagePack();
		});
	});

// Add services to the container.
builder.Services
	.AddRouting(options => options.LowercaseUrls = true)
	.AddControllers(options =>
	{
		options.Filters.Add(new ProducesAttribute(MediaTypeNames.Application.Json));
		options.Filters.Add(new ConsumesAttribute(MediaTypeNames.Application.Json));
	})
	.AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	foreach (var xmlFileName in Directory.EnumerateFiles(AppContext.BaseDirectory, "*.xml"))
		c.IncludeXmlComments(xmlFileName);
});

// Learn more about configuring Hangfire
builder
	.Services
	.AddHangfire((sp, config) =>
	{
		var systemConfig = sp.GetRequiredService<IConfiguration>();
		var connectionString = builder.Configuration.GetConnectionString("Redis")!;
		var options = new RedisStorageOptions
		{
			Prefix = "viplevels-level-schedule:hangfire:"
		};
		_ = config
		.UseRedisStorage(connectionString, options)
		.WithJobExpirationTimeout(TimeSpan.FromMinutes(30));
	})
	.AddHangfireServer();

builder.Services.AddVipLevelsLevel(
	(options, sp) =>
	{
		var configuration = sp.GetRequiredService<IConfiguration>();
		options.ConnectionString = configuration.GetConnectionString("Redis")!;
	},
	(options, sp) =>
	{
		var configuration = sp.GetRequiredService<IConfiguration>();
		options.ConnectionString = configuration.GetConnectionString("Mongo")!;
	})
	.AddVipLevelServiceClient((httpClientBuilder) =>
		httpClientBuilder.ConfigureHttpClient((sp, http) =>
		{
			var configuration = sp.GetRequiredService<IConfiguration>();
			http.BaseAddress = configuration.GetValue<Uri>("VipLevelsLevelConnectionStrings:VipLevelServiceClientBaseUrl")!;
		}))
	.AddPaymentServiceClient((httpClientBuilder) =>
		httpClientBuilder.ConfigureHttpClient((sp, http) =>
		{
			var configuration = sp.GetRequiredService<IConfiguration>();
			http.BaseAddress = configuration.GetValue<Uri>("VipLevelsLevelConnectionStrings:PaymentServiceClientBaseUrl")!;
		}));

builder.Services
	.AddKeyedSingleton("Nats", (sp, _) => new NatsConnection(NatsOpts.Default with
	{
		Url = builder.Configuration.GetConnectionString("Nats")!,
		SerializerRegistry = TgsNatsSerializerRegistration.Default
	}))
	.AddMessageQueue()
	.AddNatsMessageQueue(config => config
	.ConfigureResolveConnection(sp => sp.GetRequiredKeyedService<NatsConnection>("Nats"))
	.AddHandler<GameBetMessageHandler>(Tgs.GameBets.Worker.Common.GameBetsWorkerNatsConst.GameBetsWorkerAddPublish, typeof(GameBetMessageHandler).FullName!.ToLower())
	.AddHandler<DepositFormMessageHandler>("tgs.payment.deposit.status_ex", typeof(DepositFormMessageHandler).FullName!.ToLower())
	.AddHandler<LevelTriggerMessageHandler>(VipLevelsLevelNatsConst.VipLevelsLevelTrigger, VipLevelsLevelNatsConst.VipLevelsLevelTrigger)
	.AddHandler<LevelCheckMessageHandler>(VipLevelsLevelNatsConst.VipLevelsLevelCheck, VipLevelsLevelNatsConst.VipLevelsLevelCheck)
	.AddHandler<LevelUpgradeMessageHandler>(VipLevelsLevelNatsConst.VipLevelsLevelUpgrade, VipLevelsLevelNatsConst.VipLevelsLevelUpgrade))
	.ClearAllExchanges()
	.AddNatsGlobPatternExchange(VipLevelsLevelNatsConst.VipLevelsLevelTrigger)
	.AddNatsGlobPatternExchange(VipLevelsLevelNatsConst.VipLevelsLevelCheck)
	.AddNatsGlobPatternExchange(VipLevelsLevelNatsConst.VipLevelsLevelUpgrade);

builder.Services
	.AddOpenTelemetry()
	.WithTracing(builder => builder
		.SetResourceBuilder(ResourceBuilder.CreateDefault()
			.AddEnvironmentVariableDetector())
		.AddSource("Tgs.*")
		.AddSource("TGS.*")
		.AddSource("Valhalla.*")
		.AddAspNetCoreInstrumentation(options => options.Filter =
		httpContext => !httpContext.Request.Path.HasValue
			|| !httpContext.Request.Path.StartsWithSegments("/swagger")
			&& !httpContext.Request.Path.StartsWithSegments("/healthz")
			&& !httpContext.Request.Path.StartsWithSegments("/profiler/")
			&& !httpContext.Request.Path.StartsWithSegments("/hangfire")
			&& !httpContext.Request.Path.StartsWithSegments("/metrics")
			&& !filterInstrumentations.Any(ext => httpContext.Request.Path.Value.EndsWith($".{ext}")))
		.AddHttpClientInstrumentation()
		.AddGrpcClientInstrumentation()
		.AddGrpcCoreInstrumentation()
		.AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
		.AddOtlpExporter())
	.WithMetrics(meterBuiler => meterBuiler
	.SetResourceBuilder(ResourceBuilder.CreateDefault()
		.AddEnvironmentVariableDetector())
		.AddRuntimeInstrumentation()
		.AddAspNetCoreInstrumentation()
		.AddOtlpExporter())
	.Services
	.AddInterceptionService()
	.AddServiceUtiltiyInterceptors();

builder.Services.AddHealthChecks();

var app = builder.Build();

_ = app.UseSwagger();
_ = app.UseSwaggerUI();

app.UseHangfireDashboard(
	"/hangfire",
	new DashboardOptions()
	{
		Authorization =
		new[]
		{
			new IgnoreHangfireAuthorizationFilter()
		}
	});

_ = app.MapControllers();
_ = app.MapSwagger();

app.UseHealthChecks("/healthz");

_ = app.UseHangfireRegisterAsync();

app.Run();
