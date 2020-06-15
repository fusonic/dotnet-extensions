using Hangfire.Dashboard;

namespace Fusonic.Extensions.Hangfire
{
    public class DisableHangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context) => true;
    }
}