using System;
using System.Security.Claims;
using Fusonic.Extensions.Common.Security;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fusonic.Extensions.Hangfire.Tests
{
    public class HangfireUserAccessorDecoratorTests
    {
        [Fact]
        public void User_ReturnsOrDelegatesToInnerIfNotSet()
        {
            var innerAccessor = Substitute.For<IUserAccessor>();
            innerAccessor.User.Throws<InvalidOperationException>();
            var decorator = new HangfireUserAccessorDecorator(innerAccessor);
            Assert.Throws<InvalidOperationException>(() => decorator.User);

            decorator.User = new ClaimsPrincipal();
            Assert.NotNull(decorator.User);
        }

        [Fact]
        public void TryGetUser_ReturnsNullAndFalse_OrDelegatesToInnerIfNotSet()
        {
            var innerAccessor = Substitute.For<IUserAccessor>();
            innerAccessor.User.Throws<InvalidOperationException>();
            var decorator = new HangfireUserAccessorDecorator(innerAccessor);

            Assert.False(decorator.TryGetUser(out var nullUser));
            Assert.Null(nullUser);

            innerAccessor.ClearSubstitute();
            var claimsPrincipal = new ClaimsPrincipal();
            innerAccessor.User.Returns(claimsPrincipal);
            innerAccessor.TryGetUser(out var _).Returns(x =>
            {
                x[0] = claimsPrincipal;
                return true;
            });

            Assert.True(decorator.TryGetUser(out var user));
            Assert.NotNull(user);
        }

        [Fact]
        public void TryGetUser_ReturnsUserAndTrue_IfAvailable()
        {
            var innerAccessor = Substitute.For<IUserAccessor>();
            innerAccessor.User.Throws<InvalidOperationException>();
            var decorator = new HangfireUserAccessorDecorator(innerAccessor)
            {
                User = new ClaimsPrincipal()
            };

            Assert.True(decorator.TryGetUser(out var user));
            Assert.NotNull(user);
        }
    }
}
