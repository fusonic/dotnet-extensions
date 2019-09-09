using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SimpleInjector;

namespace Fusonic.Extensions.Hangfire
{
    public class JobProcessor : IJobProcessor
    {
        private readonly Container container;

        public JobProcessor(Container container)
            => this.container = container;

        public virtual Task ProcessAsync(HangfireJob job)
        {
            if (job.Culture != null)
                CultureInfo.CurrentCulture = job.Culture;

            if (job.UiCulture != null)
                CultureInfo.CurrentUICulture = job.UiCulture;

            var handlerType = Type.GetType(job.HandlerType, true);
            var handler = container.GetInstance(handlerType!);
            var message = job.Message;

            return InvokeAsync((dynamic)handler, (dynamic)message);
        }

        private Task InvokeAsync<TRequest, Unit>(IRequestHandler<TRequest, Unit> handler, TRequest request) where TRequest : IRequest<Unit>
            => handler.Handle(request, CancellationToken.None);

        private Task InvokeAsync<TRequest>(INotificationHandler<TRequest> handler, TRequest request) where TRequest : INotification
            => handler.Handle(request, CancellationToken.None);
    }
}