﻿// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.AspNetCore.Http.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Fusonic.Extensions.AspNetCore.Http;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseCacheHeaders(this IApplicationBuilder app, Action<CacheHeaderOptions>? configure = null)
    {
        var options = new CacheHeaderOptions();
        configure?.Invoke(options);

        return app.UseMiddleware<CacheHeaderMiddleware>(options);
    }

    /// <summary> <see cref="IgnorePathsMiddleware"/> </summary>
    public static void UseIgnorePaths(this IApplicationBuilder app, params PathString[] ignoredPaths)
        => app.UseMiddleware<IgnorePathsMiddleware>(ignoredPaths.ToList());

    /// <summary> <see cref="IgnorePathsMiddleware"/> </summary>
    public static void UseIgnorePaths(this IApplicationBuilder app, IEnumerable<PathString> ignoredPaths)
        => app.UseMiddleware<IgnorePathsMiddleware>(ignoredPaths.ToList());
}
