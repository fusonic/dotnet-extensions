using System.Security.Claims;
using Fusonic.Extensions.Common.Security;

namespace Fusonic.Extensions.Hangfire
{
    public class HangfireUserAccessorDecorator : IUserAccessor
    {
        private readonly IUserAccessor userAccessor;
        private ClaimsPrincipal? user;

        public HangfireUserAccessorDecorator(IUserAccessor userAccessor)
        {
            this.userAccessor = userAccessor;
        }

        public ClaimsPrincipal User => user ?? userAccessor.User;

        public void SetCurrentUser(ClaimsPrincipal user) => this.user = user;
    }
}
