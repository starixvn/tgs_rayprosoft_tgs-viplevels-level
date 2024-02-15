using System.Reflection;
using Tgs.VipLevels.Level.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddVipLevelsLevelCore(
		this IServiceCollection services)
		=> services
		.RegisterAllServiceUtilityTypes(
			Assembly.GetExecutingAssembly(),
			ServiceLifetime.Transient)
		.AddSingleton<IVipLevelUtensils, VipLevelUtensils>()
		.AddSingleton<ILevelUpgradeDispatcher, LevelUpgradeDispatcher>();
}