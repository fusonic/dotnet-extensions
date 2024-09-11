// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using OpenTelemetry.Trace;

namespace Fusonic.Extensions.AspNetCore.OpenTelemetry;

public static class TracerProviderBuilderExtensions
{
    /// <summary>
    /// Enables tracing for mediator requests and notifications.<br/>
    /// Note: You also need to call <see cref="ContainerExtensions.RegisterMediatorTracingDecorators"/> to enable tracing.
    /// </summary>
    public static TracerProviderBuilder AddMediatorInstrumentation(this TracerProviderBuilder builder)
        => builder.AddSource(MediatorTracer.ActivitySourceName);
}