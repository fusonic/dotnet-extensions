// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Mediator;

namespace Fusonic.Extensions.AspNetCore.OpenTelemetry;

public class TraceRequestHandlerDecorator<TRequest, TUnit>(IRequestHandler<TRequest, TUnit> requestHandler)
    : IRequestHandler<TRequest, TUnit>
    where TRequest : IRequest<TUnit>
{
    private static readonly string DisplayName = MediatorTracer.GetTypeName(typeof(TRequest));

    public async Task<TUnit> Handle(TRequest request, CancellationToken cancellationToken)
    {
        return await MediatorTracer.TraceRequest(
            requestHandler.GetType(),
            DisplayName,
            kind: "Request",
            async () => await requestHandler.Handle(request, cancellationToken));
    }
}