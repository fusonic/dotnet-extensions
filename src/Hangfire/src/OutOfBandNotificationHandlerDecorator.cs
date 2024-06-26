// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using Fusonic.Extensions.Common.Security;
using Fusonic.Extensions.Mediator;
using Hangfire;

namespace Fusonic.Extensions.Hangfire;

public class OutOfBandNotificationHandlerDecorator<TNotification, TProcessor>(INotificationHandler<TNotification> inner, IBackgroundJobClient client, RuntimeOptions runtimeOptions, IUserAccessor userAccessor) : INotificationHandler<TNotification>
    where TNotification : INotification
    where TProcessor : class, IJobProcessor
{
    public Task Handle(TNotification notification, CancellationToken cancellationToken)
    {
        if (runtimeOptions.SkipOutOfBandDecorators)
        {
            // Skipping is only important for current handler and must be disabled before invoking the handler
            // because the handler might invoke additional inner handlers which might be marked as out-of-band.
            runtimeOptions.SkipOutOfBandDecorators = false;
            return inner.Handle(notification, cancellationToken);
        }

        EnqueueHangfireJob(inner.GetType(), notification);
        return Task.CompletedTask;
    }

    private void EnqueueHangfireJob(Type handler, object notification)
    {
        var context = new MediatorHandlerContext(notification, handler.AssemblyQualifiedName!)
        {
            Culture = CultureInfo.CurrentCulture,
            UiCulture = CultureInfo.CurrentUICulture,
        };
        if (userAccessor.TryGetUser(out var user))
        {
            context.User = MediatorHandlerContext.HangfireUser.FromClaimsPrincipal(user);
        }

        client.Enqueue<TProcessor>(c => c.ProcessAsync(context, null!)); // PerformContext will be substituted by Hangfire when the job gets executed.
    }
}
