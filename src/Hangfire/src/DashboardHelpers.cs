// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Hangfire;
using Hangfire.Common;
using Hangfire.Dashboard;

namespace Fusonic.Extensions.Hangfire;

public static class DashboardHelpers
{
    /// <summary>
    /// Returns the request handlers type name if the given <paramref name="job"/> is assignable to <see cref="IJobProcessor"/>. Otherwise returns <see cref="Job.ToString"/>.
    /// </summary>
    /// <remarks>To enable job display formating, assign this method to <see cref="DashboardOptions.DisplayNameFunc"/>.</remarks>
    public static string FormatJobDisplayName(DashboardContext context, Job job)
        => job.Type.IsAssignableTo(typeof(IJobProcessor))
                        && job.Args.Count > 1
                        && job.Args[0] is MediatorHandlerContext ctx
                        && ctx.HandlerType is not null
                    ? ctx.HandlerType.Substring(0, ctx.HandlerType.IndexOf(',', StringComparison.OrdinalIgnoreCase))
                    : job.ToString();
}
