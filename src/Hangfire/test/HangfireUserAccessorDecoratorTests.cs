// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Security.Claims;
using Fusonic.Extensions.Common.Security;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ExceptionExtensions;

namespace Fusonic.Extensions.Hangfire.Tests;

public class HangfireUserAccessorDecoratorTests
{
    [Fact]
    public void User_ReturnsOrDelegatesToInnerIfNotSet()
    {
        var innerAccessor = Substitute.For<IUserAccessor>();
        innerAccessor.User.Throws<InvalidOperationException>();
        var decorator = new HangfireUserAccessorDecorator(innerAccessor);
        var act = () => decorator.User;
        act.Should().ThrowExactly<InvalidOperationException>();

        decorator.User = new ClaimsPrincipal();
        decorator.User.Should().NotBeNull();
    }

    [Fact]
    public void TryGetUser_ReturnsNullAndFalse_OrDelegatesToInnerIfNotSet()
    {
        var innerAccessor = Substitute.For<IUserAccessor>();
        innerAccessor.User.Throws<InvalidOperationException>();
        var decorator = new HangfireUserAccessorDecorator(innerAccessor);

        decorator.TryGetUser(out var nullUser).Should().BeFalse();
        nullUser.Should().BeNull();

        innerAccessor.ClearSubstitute();
        var claimsPrincipal = new ClaimsPrincipal();
        innerAccessor.User.Returns(claimsPrincipal);
        innerAccessor.TryGetUser(out var _).Returns(x =>
        {
            x[0] = claimsPrincipal;
            return true;
        });

        decorator.TryGetUser(out var user).Should().BeTrue();
        user.Should().NotBeNull();
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

        decorator.TryGetUser(out var user).Should().BeTrue();
        user.Should().NotBeNull();
    }
}
