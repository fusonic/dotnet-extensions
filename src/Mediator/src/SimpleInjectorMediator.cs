// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Collections.Concurrent;
using SimpleInjector;

namespace Fusonic.Extensions.Mediator;

public class SimpleInjectorMediator(Container container) : IMediator
{
    private static readonly ConcurrentDictionary<Type, Type> HandlerCache = new();

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var handlerType = HandlerCache.GetOrAdd(request.GetType(), t => typeof(IRequestHandler<,>).MakeGenericType(t, typeof(TResponse)));
        var handler = (IRequestHandlerBase<TResponse>)container.GetInstance(handlerType);

        return await handler.Handle(request, cancellationToken);
    }

    public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        => await Send(request, cancellationToken);

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        foreach (var handler in container.GetAllInstances<INotificationHandler<TNotification>>())
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
