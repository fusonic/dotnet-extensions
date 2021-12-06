// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Hosting.TimedHostedService;
using SimpleInjector;
using SimpleInjector.Integration.ServiceCollection;

namespace Fusonic.Extensions.Hosting;

public static class ContainerExtensions
{
    /// <summary>
    /// Registers a service to be executed by the `TimedHostedService`. The TimedHostedService executes the service repeatedly in a specified interval.
    /// </summary>
    public static void AddTimedHostedService<TService>(this SimpleInjectorAddOptions options, Action<TimedHostedServiceSettings> configureTimedHostedService, Func<TService, CancellationToken, Task> executeTask)
        where TService : class
    {
        var settings = new TimedHostedServiceSettings();
        configureTimedHostedService(settings);

        options.AddHostedService<TimedHostedService<TService>>();

        options.Container.Register<TService>();
        options.Container.RegisterInstance(new TimedHostedService<TService>.Settings(TimeSpan.FromSeconds(settings.Interval), settings.WatchdogUri, executeTask));
    }
}
