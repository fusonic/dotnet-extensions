// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.Mediator.DependencyInjection;

internal sealed class RequestHandlerWrapper<TRequest, TResponse> : IRequestHandlerWrapper
    where TRequest : IRequest<TResponse>
{
    public async Task<object> Handle(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        return (await serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>().Handle((TRequest)request, cancellationToken))!;
    }
}