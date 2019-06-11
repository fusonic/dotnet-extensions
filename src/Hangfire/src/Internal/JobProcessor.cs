using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SimpleInjector;

namespace Fusonic.Extensions.Hangfire.Internal
{
    public class JobProcessor
    {
        private readonly Container container;

        public JobProcessor(Container container)
            => this.container = container;

        public Task ProcessAsync(HangfireJob job)
        {
            if (job.Culture != null)
                CultureInfo.CurrentCulture = job.Culture;

            if (job.UiCulture != null)
                CultureInfo.CurrentUICulture = job.UiCulture;

            var handlerType = Type.GetType(job.HandlerType, true);
            var handler = container.GetInstance(handlerType);
            var message = job.Message;

            return InvokeAsync((dynamic)handler, (dynamic)message);
        }

        protected virtual Task InvokeAsync<TRequest, Unit>(IRequestHandler<TRequest, Unit> handler, TRequest request) where TRequest : IRequest<Unit>
            => handler.Handle(request, CancellationToken.None);

        protected virtual Task InvokeAsync<TRequest>(INotificationHandler<TRequest> handler, TRequest request) where TRequest : INotification
            => handler.Handle(request, CancellationToken.None);
    }
}