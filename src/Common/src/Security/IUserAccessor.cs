// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace Fusonic.Extensions.Common.Security;

/// <summary>Allows accessing the current user.</summary>
public interface IUserAccessor
{
    /// <summary>Gets the current user.</summary>
    /// <remarks>Implementation might throw an exception if now current user is avilable. Use <see cref="TryGetUser(out ClaimsPrincipal)"/> if you need an exceptionless behavior.</remarks>
    ClaimsPrincipal User { get; }

    /// <summary> Tries to get the current user.</summary>
    /// <param name="user">The retrieved user.</param>
    /// <returns><c>true</c> if a user is avilable. Otherwise <c>false</c>.</returns>
    bool TryGetUser([MaybeNullWhen(false)] out ClaimsPrincipal user);
}
