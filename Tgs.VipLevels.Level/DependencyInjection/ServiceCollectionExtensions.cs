namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddVipLevelsLevel(
		this IServiceCollection services)
		=> services
		.AddVipLevelServiceClient((httpClientBuilder) =>
		httpClientBuilder.ConfigureHttpClient((sp, http) =>
		{
			var configuration = sp.GetRequiredService<IConfiguration>();
			http.BaseAddress = configuration.GetValue<Uri>("VipLevelConnectionStrings:VipLevelServiceClientBaseUrl")!;
		}))
		.AddCalculateClient((httpClientBuilder) =>
		httpClientBuilder.ConfigureHttpClient((sp, http) =>
		{
			var configuration = sp.GetRequiredService<IConfiguration>();
			http.BaseAddress = configuration.GetValue<Uri>("VipLevelConnectionStrings:CalculateClientBaseUrl")!;
		}))
		.AddVipLevelsLevelCore();
}