// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.AspNetCore.OpenTelemetry;
using Fusonic.Extensions.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.AspNetCore.Tests.OpenTelemetry;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMediatorTracingDecorators_DecoratesHandlers()
    {
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<Command, Unit>, CommandHandler>();
        services.AddScoped<INotificationHandler<Notification>, NotificationHandler>();

        services.AddMediatorTracingDecorators();

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        scope.ServiceProvider.GetRequiredService<IRequestHandler<Command, Unit>>().Should().BeOfType<TraceRequestHandlerDecorator<Command, Unit>>();
        scope.ServiceProvider.GetRequiredService<INotificationHandler<Notification>>().Should().BeOfType<TraceNotificationHandlerDecorator<Notification>>();
    }

    public record Command : ICommand;

    public class CommandHandler : IRequestHandler<Command>
    {
        public Task<Unit> Handle(Command request, CancellationToken cancellationToken) => Unit.Task;
    }

    public record Notification : INotification;

    public class NotificationHandler : INotificationHandler<Notification>
    {
        public Task Handle(Notification notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
