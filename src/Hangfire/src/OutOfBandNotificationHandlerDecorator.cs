using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Fusonic.Extensions.Hangfire.Internal;
using Hangfire;
using MediatR;

namespace Fusonic.Extensions.Hangfire
{
    public class OutOfBandNotificationHandlerDecorator<TNotification> : INotificationHandler<TNotification> where TNotification : INotification
    {
        private readonly INotificationHandler<TNotification> inner;
        private readonly IBackgroundJobClient client;

        public OutOfBandNotificationHandlerDecorator(INotificationHandler<TNotification> inner, IBackgroundJobClient client)
        {
            this.inner = inner;
            this.client = client;
        }

        public Task Handle(TNotification notification, CancellationToken cancellationToken)
        {
            EnqueueHangfireJob(inner.GetType(), notification);
            return Task.CompletedTask;
        }

        private void EnqueueHangfireJob(Type handler, object notification)
        {
            var job = new HangfireJob
            {
                Message = notification,
                HandlerType = handler.AssemblyQualifiedName,
                Culture = CultureInfo.CurrentCulture,
                UiCulture = CultureInfo.CurrentUICulture
            };

            client.Enqueue<JobProcessor>(c => c.ProcessAsync(job));
        }
    }
}