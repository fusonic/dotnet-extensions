// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Fusonic.Extensions.Common.Security;

namespace Fusonic.Extensions.Hangfire;

public class HangfireUserAccessorDecorator : IUserAccessor
{
    private readonly IUserAccessor userAccessor;
    private ClaimsPrincipal? user;

    public HangfireUserAccessorDecorator(IUserAccessor userAccessor)
        => this.userAccessor = userAccessor;

    public ClaimsPrincipal User
    {
        get => user ?? userAccessor.User;
        set => user = value;
    }

    public bool TryGetUser([MaybeNullWhen(false)] out ClaimsPrincipal user)
    {
        user = this.user;
        if (user == null)
        {
            return userAccessor.TryGetUser(out user);
        }
        else
        {
            return true;
        }
    }
}
