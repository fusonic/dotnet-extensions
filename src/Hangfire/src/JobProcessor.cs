// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using SimpleInjector;

namespace Fusonic.Extensions.Hangfire;

/// <summary> Processes out-of-band jobs using SimpleInjector. </summary>
public class JobProcessor(Container container) : JobProcessorBase
{
    protected override object GetService(Type serviceType)
        => container.GetInstance(serviceType);

    protected override object GetNotificationHandler(Type notificationType, Type handlerType)
    {
        var dispatcher = (INotificationDispatcher)container.GetInstance(typeof(NotificationDispatcher<>).MakeGenericType(notificationType));
        return dispatcher.GetHandler(handlerType);
    }
}
