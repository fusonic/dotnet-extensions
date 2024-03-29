// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using Fusonic.Extensions.Common.Security;
using Fusonic.Extensions.Mediator;
using Hangfire;

namespace Fusonic.Extensions.Hangfire;

public class OutOfBandRequestHandlerDecorator<TRequest, TProcessor>(IRequestHandler<TRequest, Unit> inner, IBackgroundJobClient client, RuntimeOptions runtimeOptions, IUserAccessor userAccessor) : IRequestHandler<TRequest, Unit>
    where TRequest : IRequest<Unit>
    where TProcessor : class, IJobProcessor
{
    public Task<Unit> Handle(TRequest request, CancellationToken cancellationToken)
    {
        if (runtimeOptions.SkipOutOfBandDecorators)
        {
            // Skipping is only important for current handler and must be disabled before invoking the handler
            // because the handler might invoke additional inner handlers which might be marked as out-of-band.
            runtimeOptions.SkipOutOfBandDecorators = false;
            return inner.Handle(request, cancellationToken);
        }

        EnqueueHangfireJob(inner.GetType(), request);
        return Task.FromResult(Unit.Value);
    }

    private void EnqueueHangfireJob(Type handler, object command)
    {
        var context = new MediatorHandlerContext(command, handler.AssemblyQualifiedName!)
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
