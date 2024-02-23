using MongoDB.Driver;
using Tgs.VipLevels.Level.Core;
using Tgs.VipLevels.Level.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddVipLevelsLevelRepositories(
		this IServiceCollection services,
		Func<IServiceProvider, IMongoDatabase> mongoDatabaseResolver)
		=> services
		.AddTransient<IPlayerVipLevelBacklogRepository>((sp) => ActivatorUtilities.CreateInstance<PlayerVipLevelBacklogRepository>(sp, mongoDatabaseResolver(sp)));
}
