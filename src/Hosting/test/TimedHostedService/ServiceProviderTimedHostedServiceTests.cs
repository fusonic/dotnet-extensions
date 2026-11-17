// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Hosting.TimedHostedService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fusonic.Extensions.Hosting.Tests.TimedHostedService;

public class ServiceProviderTimedHostedServiceTests
{
    [Fact]
    public async Task AddTimedHostedService_RunsServiceInScope()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTimedHostedService<ScopedService>(_ => { }, (s, _) => s.Run());

        await using var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        using var timedHostedService = serviceProvider.GetServices<IHostedService>().OfType<ServiceProviderTimedHostedService<ScopedService>>().Single();

        // Act
        await timedHostedService.StartAsync(CancellationToken.None);
        await timedHostedService.ExecutingTask;
        await timedHostedService.StopAsync(CancellationToken.None);

        // Assert
        ScopedService.RunCount.Should().Be(1);
    }

    public class ScopedService
    {
        public static int RunCount { get; private set; }

        public Task Run()
        {
            RunCount++;
            return Task.CompletedTask;
        }
    }
}
