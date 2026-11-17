// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Logging;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Fusonic.Extensions.Hosting.TimedHostedService;

/// <summary> Executes a service repeatedly in a specified interval using SimpleInjector. </summary>
public class TimedHostedService<TService>(Container container, ILogger<TimedHostedService<TService>> logger, TimedHostedServiceBase<TService>.Settings settings, IHttpClientFactory httpClientFactory)
    : TimedHostedServiceBase<TService>(logger, settings, httpClientFactory)
    where TService : class
{
    protected override async Task RunInScopeAsync(Func<TService, Task> action)
    {
        await using (AsyncScopedLifestyle.BeginScope(container))
        {
            await action(container.GetInstance<TService>());
        }
    }
}
