using Hangfire.Dashboard;

namespace Tgs.VipLevels.Level;

public class IgnoreHangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
	public bool Authorize(DashboardContext context) => true;
}
