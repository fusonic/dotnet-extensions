// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.AspNetCore.OpenTelemetry;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds decorators for tracing mediator requests and notifications using Scrutor.<br/>
    /// Note: The decorators only apply to handlers registered before. You also need to call <see cref="TracerProviderBuilderExtensions.AddMediatorInstrumentation"/> to enable tracing.
    /// </summary>
    public static IServiceCollection AddMediatorTracingDecorators(this IServiceCollection services)
    {
        services.TryDecorate(typeof(IRequestHandler<,>), typeof(TraceRequestHandlerDecorator<,>));
        services.TryDecorate(typeof(INotificationHandler<>), typeof(TraceNotificationHandlerDecorator<>));
        return services;
    }
}
