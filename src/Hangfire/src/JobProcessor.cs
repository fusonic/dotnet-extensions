// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using Fusonic.Extensions.Common.Security;
using Fusonic.Extensions.Mediator;
using Hangfire;
using Hangfire.Server;
using SimpleInjector;

namespace Fusonic.Extensions.Hangfire;

public class JobProcessor(Container container) : IJobProcessor
{
    [JobDisplayName("OutOfBand {0}")]
    public virtual Task ProcessAsync(MediatorHandlerContext context, PerformContext performContext)
    {
        if (context.Culture != null)
            CultureInfo.CurrentCulture = context.Culture;

        if (context.UiCulture != null)
            CultureInfo.CurrentUICulture = context.UiCulture;

        var userAccessor = container.GetInstance<IUserAccessor>();
        if (context.User != null && userAccessor is HangfireUserAccessorDecorator decorator)
            decorator.User = context.User.ToClaimsPrincipal();

        var message = context.Message;
        var messageType = message.GetType();

        // Skip out of band decorators to avoid recursion (RuntimeOptions is registered as scoped, so OutOfBandDecorators get the same instance).
        var runtimeOptions = container.GetInstance<RuntimeOptions>();
        runtimeOptions.SkipOutOfBandDecorators = true;

        if (message is INotification)
        {
            var dispatcherType = typeof(NotificationDispatcher<>).MakeGenericType(messageType);
            var dispatcher = container.GetInstance(dispatcherType);
            var handlerType = Type.GetType(context.HandlerType, true);
            return InvokeAsync((dynamic)dispatcher, handlerType, (dynamic)message);
        }
        else if (message is IRequest<Unit>)
        {
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType([messageType, typeof(Unit)]);
            var handler = container.GetInstance(handlerType);
            return InvokeAsync((dynamic)handler, (dynamic)message);
        }
        else
        {
            throw new InvalidOperationException($"Could not process message. Message instance must implement `{nameof(INotification)}` or `{nameof(IRequest<Unit>)}`.");
        }
    }

    private static Task<Unit> InvokeAsync<TRequest>(IRequestHandler<TRequest, Unit> handler, TRequest request) where TRequest : IRequest<Unit>
        => handler.Handle(request, CancellationToken.None);

    private static Task InvokeAsync<TRequest>(NotificationDispatcher<TRequest> dispatcher, Type handlerType, TRequest request) where TRequest : INotification
        => dispatcher.Dispatch(request, handlerType, CancellationToken.None);
}
