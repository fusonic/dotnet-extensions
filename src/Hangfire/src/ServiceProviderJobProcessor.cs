// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.Hangfire;

/// <summary> Processes out-of-band jobs using Microsoft.Extensions.DependencyInjection. </summary>
public class ServiceProviderJobProcessor(IServiceProvider serviceProvider) : JobProcessorBase
{
    protected override object GetService(Type serviceType)
        => serviceProvider.GetRequiredService(serviceType);

    protected override object GetNotificationHandler(Type notificationType, Type handlerType)
        => serviceProvider.GetRequiredService<OutOfBandNotificationHandlerRegistry>()
                          .GetHandler(serviceProvider, typeof(INotificationHandler<>).MakeGenericType(notificationType), handlerType);
}
