using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Fusonic.Extensions.Common.Security;
using Microsoft.AspNetCore.Http;

namespace Fusonic.Extensions.AspNetCore.Http
{
    public class HttpContextUserAccessor : IUserAccessor
    {
        private readonly IHttpContextAccessor contextAccessor;

        public HttpContextUserAccessor(IHttpContextAccessor contextAccessor)
            => this.contextAccessor = contextAccessor;

        public ClaimsPrincipal User
            => (contextAccessor.HttpContext ?? throw new InvalidOperationException("No HttpContext available.")).User;

        public bool TryGetUser([MaybeNullWhen(false)] out ClaimsPrincipal user)
        {
            user = contextAccessor.HttpContext?.User;
            return user != null;
        }
    }
}