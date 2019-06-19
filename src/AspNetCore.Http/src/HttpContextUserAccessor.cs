using System.Security.Claims;
using Fusonic.Extensions.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Fusonic.Extensions.AspNetCore.Http
{
    public class HttpContextUserAccessor : IUserAccessor
    {
        private readonly IHttpContextAccessor contextAccessor;

        public HttpContextUserAccessor(IHttpContextAccessor contextAccessor)
            => this.contextAccessor = contextAccessor;

        public ClaimsPrincipal User => contextAccessor.HttpContext.User;
    }
}