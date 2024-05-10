using Hangfire.Dashboard;

namespace Tgs.GameRecord.VRGaming;

public class IgnoreHangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
	public bool Authorize(DashboardContext context) => true;
}
