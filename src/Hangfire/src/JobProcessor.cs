using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.Server;
using MediatR;
using SimpleInjector;

namespace Fusonic.Extensions.Hangfire
{
    public class JobProcessor : IJobProcessor
    {
        private readonly Container container;

        public JobProcessor(Container container)
            => this.container = container;

        public virtual Task ProcessAsync(MediatorHandlerContext context, PerformContext performContext)
        {
            if (context.Culture != null)
                CultureInfo.CurrentCulture = context.Culture;

            if (context.UiCulture != null)
                CultureInfo.CurrentUICulture = context.UiCulture;

            var handlerType = Type.GetType(context.HandlerType, true);
            var handler = container.GetInstance(handlerType!);
            var message = context.Message;

            return InvokeAsync((dynamic)handler, (dynamic)message);
        }

        private Task InvokeAsync<TRequest, Unit>(IRequestHandler<TRequest, Unit> handler, TRequest request) where TRequest : IRequest<Unit>
            => handler.Handle(request, CancellationToken.None);

        private Task InvokeAsync<TRequest>(INotificationHandler<TRequest> handler, TRequest request) where TRequest : INotification
            => handler.Handle(request, CancellationToken.None);
    }
}