using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using MediatR;

namespace Fusonic.Extensions.Hangfire
{
    public class OutOfBandNotificationHandlerDecorator<TNotification, TProcessor> : INotificationHandler<TNotification>
        where TNotification : INotification
        where TProcessor : class, IJobProcessor
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
            var job = new HangfireJob(notification, handler.AssemblyQualifiedName!)
            {
                Culture = CultureInfo.CurrentCulture,
                UiCulture = CultureInfo.CurrentUICulture
            };

            client.Enqueue<TProcessor>(c => c.ProcessAsync(job));
        }
    }
}