// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Net;
using FluentAssertions;
using Fusonic.Extensions.Hosting.TimedHostedService;
using Fusonic.Extensions.UnitTests;
using NSubstitute;
using SimpleInjector;
using Xunit;

namespace Fusonic.Extensions.Hosting.Tests.TimedHostedService;

public class TimedHostedServiceTests : DependencyInjectionUnitTest<TimedHostedServiceTests.TimedHostedServiceFixture>
{
    public TimedHostedServiceTests(TimedHostedServiceFixture fixture) : base(fixture) => GetInstance<Service>().Reset();

    [Fact]
    public async Task Start_RunsTask_ImmediatelyAfterStart()
    {
        // Arrange
        using var timedHostedService = GetInstance<TimedHostedService<Service>>();

        // Act
        await timedHostedService.StartAsync(CancellationToken.None);
        await timedHostedService.ExecutingTask;

        // Assert
        var service = GetInstance<Service>();
        service.RunCalled.Should().BeTrue();
    }

    [Fact]
    public async Task RunTask_CrashesOnStartup_TimedHostedServiceDoesNotCare()
    {
        using var timedHostedService = GetInstance<TimedHostedService<ServiceCrashing>>();
        var act = () => timedHostedService.StartAsync(CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RunTask_CallsWatchdog_WhenConfigured()
    {
        // Arrange
        using var timedHostedService = GetInstance<TimedHostedService<Service>>();
        var messageHandler = GetInstance<HttpMessageHandlerMock>();
        Uri? requestUri = null;
        messageHandler.CallAction = msg => requestUri = msg.RequestUri;

        // Act
        await timedHostedService.StartAsync(CancellationToken.None);
        await timedHostedService.ExecutingTask;

        // Assert
        messageHandler.GotRequest.Should().BeTrue();
        requestUri.Should().NotBeNull();
        requestUri!.AbsoluteUri.Should().Be("https://woof.woof/");
    }

    [Fact]
    public async Task RunTask_DoesNotCallWatchdog_WhenNotConfigured()
    {
        // Arrange
        using var timedHostedService = GetInstance<TimedHostedService<ServiceNoWatchdog>>();
        var messageHandler = GetInstance<HttpMessageHandlerMock>();

        // Act
        await timedHostedService.StartAsync(CancellationToken.None);

        // Assert
        messageHandler.GotRequest.Should().BeFalse();
    }

    [Fact]
    public async Task RunTask_DoesNotCallWatchdog_WhenCrashing()
    {
        // Arrange
        using var timedHostedService = GetInstance<TimedHostedService<ServiceCrashing>>();
        var messageHandler = GetInstance<HttpMessageHandlerMock>();

        // Act
        await timedHostedService.StartAsync(CancellationToken.None);

        // Assert
        messageHandler.GotRequest.Should().BeFalse();
    }

    [Fact]
    public async Task Stop_CancelsCancellationToken()
    {
        // Arrange
        using var timedHostedService = GetInstance<TimedHostedService<Service>>();
        await timedHostedService.StartAsync(CancellationToken.None);

        // Act
        await timedHostedService.StopAsync(CancellationToken.None);

        // Assert
        var service = GetInstance<Service>();
        service.CancellationToken.IsCancellationRequested.Should().BeTrue();
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(0, true)]
    [InlineData(-1, true)]
    public void ConfigRequiresValidInterval(int interval, bool shouldThrow)
    {
        var act = () => new TimedHostedService<Service>.Settings(new TimeSpan(interval), watchdogUri: null, (s, c) => s.Run(c));

        if (shouldThrow)
            act.Should().Throw<ArgumentException>();
        else
            act.Should().NotThrow();
    }

    private class Service
    {
        public bool RunCalled { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public void Reset()
        {
            CancellationToken = CancellationToken.None;
            RunCalled = false;
        }

        public Task Run(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            RunCalled = true;
            return Task.CompletedTask;
        }
    }

    private sealed class ServiceNoWatchdog : Service
    { }

    private sealed class ServiceCrashing
    {
#pragma warning disable CA1822 // Mark members as static
        public Task Run() => throw new IOException("Could not find directory /mnt/uRock/uRule");
#pragma warning restore CA1822 // Mark members as static
    }

    public class HttpMessageHandlerMock
        : HttpMessageHandler
    {
        public Action<HttpRequestMessage>? CallAction { get; set; }
        public bool GotRequest { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            GotRequest = true;
            CallAction?.Invoke(request);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    public class TimedHostedServiceFixture : TestFixture
    {
        protected override void RegisterDependencies(Container container)
        {
            AddTimedHostedService<Service>(s => s.WatchdogUri = new Uri("https://woof.woof"), (s, c) => s.Run(c));
            AddTimedHostedService<ServiceNoWatchdog>(_ => { }, (s, c) => s.Run(c));
            AddTimedHostedService<ServiceCrashing>(_ => { }, (s, _) => s.Run());

            container.Register<HttpMessageHandlerMock>();
            container.Register(() =>
            {
                var httpClientFactory = Substitute.For<IHttpClientFactory>();
                var messageHandlerMock = container.GetInstance<HttpMessageHandlerMock>();
                httpClientFactory.CreateClient("").ReturnsForAnyArgs(new HttpClient(messageHandlerMock));
                return httpClientFactory;
            });

            void AddTimedHostedService<TService>(Action<TimedHostedServiceSettings> configureTimedHostedService, Func<TService, CancellationToken, Task> executeTask)
                where TService : class
            {
                var settings = new TimedHostedServiceSettings();
                configureTimedHostedService(settings);
                var hostSettings = new TimedHostedService<TService>.Settings(TimeSpan.FromSeconds(settings.Interval), settings.WatchdogUri, executeTask);

                container.Register<TimedHostedService<TService>>();
                container.RegisterSingleton<TService>();
                container.Register(() => hostSettings);
            }
        }
    }
}
