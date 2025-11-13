// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Fusonic.Extensions.Mediator.DependencyInjection;

public class ServiceProviderMediator(IServiceProvider serviceProvider) : IMediator
{
    private static readonly ConcurrentDictionary<Type, IRequestHandlerWrapper> RequestWrappers = new();
    private static readonly ConcurrentDictionary<Type, INotificationHandlerWrapper> NotificationWrappers = new();
    private static readonly ConcurrentDictionary<Type, IAsyncEnumerableRequestHandlerWrapper> AsyncEnumWrappers = new();

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wrapper = RequestWrappers.GetOrAdd(request.GetType(),
            t =>
            {
                var wrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(t, typeof(TResponse));
                var wrapperInstance = Activator.CreateInstance(wrapperType) as IRequestHandlerWrapper
                                   ?? throw new ActivationException($"Could not create wrapper for type {t.FullName}.");

                return wrapperInstance;
            });

        return (TResponse)await wrapper.Handle(request, serviceProvider, cancellationToken);
    }

    public async Task Publish(INotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var wrapper = NotificationWrappers.GetOrAdd(notification.GetType(),
            t =>
            {
                var wrapperType = typeof(NotificationHandlerWrapper<>).MakeGenericType(t);
                var wrapperInstance = Activator.CreateInstance(wrapperType) as INotificationHandlerWrapper
                                   ?? throw new ActivationException($"Could not create wrapper for type {t.FullName}.");

                return wrapperInstance;
            });

        await wrapper.Handle(notification, serviceProvider, cancellationToken);
    }

    public async IAsyncEnumerable<TResponse> CreateAsyncEnumerable<TResponse>(IAsyncEnumerableRequest<TResponse> request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wrapper = AsyncEnumWrappers.GetOrAdd(request.GetType(),
            t =>
            {
                var wrapperType = typeof(AsyncEnumerableRequestHandlerWrapper<,>).MakeGenericType(t, typeof(TResponse));
                var wrapperInstance = Activator.CreateInstance(wrapperType) as IAsyncEnumerableRequestHandlerWrapper
                                   ?? throw new ActivationException($"Could not create wrapper for type {t.FullName}.");

                return wrapperInstance;
            });

        await foreach (var item in wrapper.Handle(request, serviceProvider, cancellationToken))
        {
            yield return (TResponse)item;
        }
    }
}
