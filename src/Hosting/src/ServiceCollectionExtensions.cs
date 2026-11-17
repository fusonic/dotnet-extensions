// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Hosting.TimedHostedService;
using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.Hosting;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a service to be executed by the <see cref="ServiceProviderTimedHostedService{TService}"/>, which executes the service repeatedly in a specified interval.
    /// The service is registered as scoped and resolved within a new scope on each execution.
    /// </summary>
    public static IServiceCollection AddTimedHostedService<TService>(this IServiceCollection services, Action<TimedHostedServiceSettings> configureTimedHostedService, Func<TService, CancellationToken, Task> executeTask)
        where TService : class
    {
        var settings = new TimedHostedServiceSettings();
        configureTimedHostedService(settings);

        services.AddHttpClient();
        services.AddHostedService<ServiceProviderTimedHostedService<TService>>();

        services.AddScoped<TService>();
        services.AddSingleton(new TimedHostedServiceBase<TService>.Settings(TimeSpan.FromSeconds(settings.Interval), settings.WatchdogUri, executeTask));

        return services;
    }
}
