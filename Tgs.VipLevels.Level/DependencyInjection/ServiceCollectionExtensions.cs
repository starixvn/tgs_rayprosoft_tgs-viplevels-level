using Tgs.VipLevels.Level;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddVipLevelsLevel(
		this IServiceCollection services,
		Action<RedisOptions, IServiceProvider> redisOptions,
		Action<MongoDBOptions, IServiceProvider> mongoDBconfigure)
		=> services
		.AddOptions<RedisOptions>()
		.Configure(redisOptions)
		.Services
		.AddOptions<MongoDBOptions>()
		.Configure(mongoDBconfigure)
		.Services
		.AddSingleton<RedisConnectionManager>()
		.AddSingleton<MongoDBConnectionManager>()
		.AddSingleton(TimeProvider.System)
		.AddVipLevelsLevelCore()
		.AddVipLevelsLevelRepositories((sp) => sp.GetRequiredService<MongoDBConnectionManager>().MongoDatabase);
}
