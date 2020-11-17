// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Hangfire.Dashboard;

namespace Fusonic.Extensions.Hangfire
{
    public class DisableHangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context) => true;
    }
}