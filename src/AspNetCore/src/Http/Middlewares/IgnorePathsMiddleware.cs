// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Net;
using Microsoft.AspNetCore.Http;

namespace Fusonic.Extensions.AspNetCore.Http.Middlewares;

/// <summary>
/// Returns 404 for all configured paths.
/// This is useful if you, for example, want to avoid a SPA to handle a path. A typo in an API-Url should result in 404 and should not be handled by the SPA.
/// </summary>
public class IgnorePathsMiddleware(RequestDelegate next, List<PathString> ignoredNormalizedPaths)
{
    public Task Invoke(HttpContext context)
    {
        var requestPath = context.Request.Path;

        foreach (var ignoredPath in ignoredNormalizedPaths)
        {
            if (requestPath.StartsWithSegments(ignoredPath))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Task.CompletedTask;
            }
        }

        return next(context);
    }
}
