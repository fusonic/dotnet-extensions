// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Fusonic.Extensions.Common.Security;
using Microsoft.AspNetCore.Http;

namespace Fusonic.Extensions.AspNetCore.Http;

public class HttpContextUserAccessor(IHttpContextAccessor contextAccessor) : IUserAccessor
{
    public ClaimsPrincipal User
        => (contextAccessor.HttpContext ?? throw new InvalidOperationException("No HttpContext available.")).User;

    public bool TryGetUser([MaybeNullWhen(false)] out ClaimsPrincipal user)
    {
        user = contextAccessor.HttpContext?.User;
        return user != null;
    }
}
