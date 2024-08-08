// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using SimpleInjector;

namespace Fusonic.Extensions.Mediator.SimpleInjector;

internal sealed class RequestHandlerWrapper<TRequest, TResponse> : IRequestHandlerWrapper
    where TRequest : IRequest<TResponse>
{
    public async Task<object> Handle(object request, Container container, CancellationToken cancellationToken)
    {
        return (await container.GetInstance<IRequestHandler<TRequest, TResponse>>().Handle((TRequest)request, cancellationToken))!;
    }
}