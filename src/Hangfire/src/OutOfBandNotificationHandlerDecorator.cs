﻿using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Fusonic.Extensions.Common.Security;
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
        private readonly RuntimeOptions runtimeOptions;
        private readonly IUserAccessor userAccessor;

        public OutOfBandNotificationHandlerDecorator(INotificationHandler<TNotification> inner, IBackgroundJobClient client, RuntimeOptions runtimeOptions, IUserAccessor userAccessor)
        {
            this.inner = inner;
            this.client = client;
            this.runtimeOptions = runtimeOptions;
            this.userAccessor = userAccessor;
        }

        public Task Handle(TNotification notification, CancellationToken cancellationToken)
        {
            if (runtimeOptions.SkipOutOfBandDecorators)
            {
                // Skipping is only important for current handler and must be disabled before invoking the handler
                // because the handler might invoke additional inner handlers which might be marked as out-of-band.
                runtimeOptions.SkipOutOfBandDecorators = false;
                return inner.Handle(notification, cancellationToken);
            }
            else
            {
                EnqueueHangfireJob(inner.GetType(), notification);
                return Task.CompletedTask;
            }
        }

        private void EnqueueHangfireJob(Type handler, object notification)
        {
            var context = new MediatorHandlerContext(notification, handler.AssemblyQualifiedName!)
            {
                Culture = CultureInfo.CurrentCulture,
                UiCulture = CultureInfo.CurrentUICulture,
                User = MediatorHandlerContext.HangfireUser.FromClaimsPrincipal(userAccessor.User)
            };

            client.Enqueue<TProcessor>(c => c.ProcessAsync(context, null!)); // PerformContext will be substituted by Hangfire when the job gets executed.
        }
    }
}