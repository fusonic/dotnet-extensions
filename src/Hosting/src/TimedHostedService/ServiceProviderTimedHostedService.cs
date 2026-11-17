// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fusonic.Extensions.Hosting.TimedHostedService;

/// <summary> Executes a service repeatedly in a specified interval using Microsoft.Extensions.DependencyInjection. </summary>
public class ServiceProviderTimedHostedService<TService>(IServiceScopeFactory scopeFactory, ILogger<ServiceProviderTimedHostedService<TService>> logger, TimedHostedServiceBase<TService>.Settings settings, IHttpClientFactory httpClientFactory)
    : TimedHostedServiceBase<TService>(logger, settings, httpClientFactory)
    where TService : class
{
    protected override async Task RunInScopeAsync(Func<TService, Task> action)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        await action(scope.ServiceProvider.GetRequiredService<TService>());
    }
}
