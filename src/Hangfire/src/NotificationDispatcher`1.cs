// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Mediator;
using SimpleInjector;

namespace Fusonic.Extensions.Hangfire;

internal interface INotificationDispatcher
{
    object GetHandler(Type handlerType);
}

internal sealed class NotificationDispatcher<T>(IList<DependencyMetadata<INotificationHandler<T>>> metadata) : INotificationDispatcher
    where T : INotification
{
    private readonly Dictionary<Type, DependencyMetadata<INotificationHandler<T>>> metadataDictionary = metadata.ToDictionary(p => p.ImplementationType);

    public object GetHandler(Type handlerType) => metadataDictionary[handlerType].GetInstance();
}
