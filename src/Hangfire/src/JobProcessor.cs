using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Fusonic.Extensions.Common.Security;
using Hangfire.Server;
using MediatR;
using SimpleInjector;

namespace Fusonic.Extensions.Hangfire
{
    public class JobProcessor : IJobProcessor
    {
        private readonly Container container;

        public JobProcessor(Container container)
        {
            this.container = container;
        }

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
                var handlerType = typeof(IRequestHandler<,>).MakeGenericType(new[] { messageType, typeof(Unit) });
                var handler = container.GetInstance(handlerType);
                return InvokeAsync((dynamic)handler, (dynamic)message);
            }
            else
            {
                throw new InvalidOperationException($"Could not process message. Message instance must implement `{nameof(INotification)}` or `{nameof(IRequest<Unit>)}`.");
            }
        }

        private Task InvokeAsync<TRequest>(IRequestHandler<TRequest, Unit> handler, TRequest request) where TRequest : IRequest<Unit>
            => handler.Handle(request, CancellationToken.None);

        private Task InvokeAsync<TRequest>(NotificationDispatcher<TRequest> dispatcher, Type handlerType, TRequest request) where TRequest : INotification
            => dispatcher.Dispatch(request, handlerType, CancellationToken.None);
    }
}