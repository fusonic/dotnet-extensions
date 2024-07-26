// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Collections.Concurrent;
using SimpleInjector;

namespace Fusonic.Extensions.Mediator;

public class SimpleInjectorMediator(Container container) : IMediator
{
    private static readonly ConcurrentDictionary<Type, Type> RequestHandlerCache = [];
    private static readonly ConcurrentDictionary<Type, Type> NotificationHandlerCache = [];

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var handlerType = RequestHandlerCache.GetOrAdd(request.GetType(), static t => typeof(IRequestHandler<,>).MakeGenericType(t, typeof(TResponse)));
        var handler = (IRequestHandlerBase<TResponse>)container.GetInstance(handlerType);

        return await handler.Handle(request, cancellationToken);
    }

    public async Task Publish(INotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var handlerType = NotificationHandlerCache.GetOrAdd(notification.GetType(), static t => typeof(INotificationHandler<>).MakeGenericType(t));

        foreach (var handler in container.GetAllInstances(handlerType).Cast<INotificationHandlerBase>())
        {
            await handler.Handle(notification, cancellationToken);
        }
    }

    public IAsyncEnumerable<TResponse> CreateAsyncEnumerable<TResponse>(IAsyncEnumerableRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var handlerType = typeof(IAsyncEnumerableRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));

        var handler = (IAsyncEnumerableRequestHandlerBase<TResponse>)container.GetInstance(handlerType);
        return handler.Handle(request, cancellationToken);
    }
}