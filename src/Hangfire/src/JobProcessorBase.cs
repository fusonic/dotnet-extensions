// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using Fusonic.Extensions.Common.Security;
using Fusonic.Extensions.Mediator;
using Hangfire;
using Hangfire.Server;

namespace Fusonic.Extensions.Hangfire;

/// <summary> Processes out-of-band jobs, independent of the used DI container. </summary>
public abstract class JobProcessorBase : IJobProcessor
{
    [JobDisplayName("OutOfBand {0}")]
    public virtual Task ProcessAsync(MediatorHandlerContext context, PerformContext performContext)
    {
        if (context.Culture != null)
            CultureInfo.CurrentCulture = context.Culture;

        if (context.UiCulture != null)
            CultureInfo.CurrentUICulture = context.UiCulture;

        var userAccessor = (IUserAccessor)GetService(typeof(IUserAccessor));
        if (context.User != null && userAccessor is HangfireUserAccessorDecorator decorator)
            decorator.User = context.User.ToClaimsPrincipal();

        var message = context.Message;
        var messageType = message.GetType();

        // Skip out of band decorators to avoid recursion (RuntimeOptions is registered as scoped, so OutOfBandDecorators get the same instance).
        var runtimeOptions = (RuntimeOptions)GetService(typeof(RuntimeOptions));
        runtimeOptions.SkipOutOfBandDecorators = true;

        if (message is INotification)
        {
            var handlerType = Type.GetType(context.HandlerType, true)!;
            var handler = GetNotificationHandler(messageType, handlerType);
            return InvokeAsync((dynamic)handler, (dynamic)message);
        }
        else if (message is IRequest<Unit>)
        {
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType([messageType, typeof(Unit)]);
            var handler = GetService(handlerType);
            return InvokeAsync((dynamic)handler, (dynamic)message);
        }
        else
        {
            throw new InvalidOperationException($"Could not process message. Message instance must implement `{nameof(INotification)}` or `{nameof(IRequest<Unit>)}`.");
        }
    }

    /// <summary> Resolves a service from the current scope. </summary>
    protected abstract object GetService(Type serviceType);

    /// <summary> Resolves the <see cref="INotificationHandler{TNotification}"/> implemented by <paramref name="handlerType"/>, including its decorators. </summary>
    protected abstract object GetNotificationHandler(Type notificationType, Type handlerType);

    private static Task<Unit> InvokeAsync<TRequest>(IRequestHandler<TRequest, Unit> handler, TRequest request) where TRequest : IRequest<Unit>
        => handler.Handle(request, CancellationToken.None);

    private static Task InvokeAsync<TNotification>(INotificationHandler<TNotification> handler, TNotification notification) where TNotification : INotification
        => handler.Handle(notification, CancellationToken.None);
}
