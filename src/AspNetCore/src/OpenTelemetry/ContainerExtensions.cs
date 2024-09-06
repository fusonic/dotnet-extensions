// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Mediator;
using SimpleInjector;

namespace Fusonic.Extensions.AspNetCore.OpenTelemetry;

public static class ContainerExtensions
{
    /// <summary>
    /// Adds decorators for tracing mediator requests and notifications.
    /// </summary>
    /// <param name="container">The SimpleInjector container</param>
    public static void RegisterMediatorTracingDecorators(this Container container)
    {
        container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(TraceRequestHandlerDecorator<,>));
        container.RegisterDecorator(typeof(INotificationHandler<>), typeof(TraceNotificationHandlerDecorator<>));
    }
}