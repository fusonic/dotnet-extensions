using System;
using Fusonic.Extensions.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace Fusonic.Extensions.AspNetCore.Tests.Http
{
    public class HttpContextUserAccessorTests
    {
        [Fact]
        public void User_ThrowsIfNotAvailable()
        {
            var httpContextAccessor = new HttpContextAccessor();
            var userAccessor = new HttpContextUserAccessor(httpContextAccessor);
            Assert.Throws<InvalidOperationException>(() => userAccessor.User);
        }

        [Fact]
        public void User_ReturnsCurrentUser()
        {
            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = Substitute.For<HttpContext>()
            };
            var userAccessor = new HttpContextUserAccessor(httpContextAccessor);
            Assert.NotNull(userAccessor.User);
        }

        [Fact]
        public void TryGetUser_VerifyBehaviour()
        {
            var httpContextAccessor = new HttpContextAccessor();
            var userAccessor = new HttpContextUserAccessor(httpContextAccessor);
            Assert.False(userAccessor.TryGetUser(out var nullUser));
            Assert.Null(nullUser);

            httpContextAccessor.HttpContext = Substitute.For<HttpContext>();
            Assert.True(userAccessor.TryGetUser(out var user));
            Assert.NotNull(user);
        }
    }
}