// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Fusonic.Extensions.AspNetCore.Tests.Http;

public class HttpContextUserAccessorTests
{
    [Fact]
    public void User_ThrowsIfNotAvailable()
    {
        var httpContextAccessor = new HttpContextAccessor();
        var userAccessor = new HttpContextUserAccessor(httpContextAccessor);
        var act = () => userAccessor.User;
        act.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void User_ReturnsCurrentUser()
    {
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = Substitute.For<HttpContext>()
        };

        var userAccessor = new HttpContextUserAccessor(httpContextAccessor);
        userAccessor.User.Should().NotBeNull();
    }

    [Fact]
    public void TryGetUser_VerifyBehaviour()
    {
        var httpContextAccessor = new HttpContextAccessor();
        var userAccessor = new HttpContextUserAccessor(httpContextAccessor);
        userAccessor.TryGetUser(out var nullUser).Should().BeFalse();
        nullUser.Should().BeNull();

        httpContextAccessor.HttpContext = Substitute.For<HttpContext>();
        userAccessor.TryGetUser(out var user).Should().BeTrue();
        user.Should().NotBeNull();
    }
}