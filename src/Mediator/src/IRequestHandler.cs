// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Mediator;

/// <summary>
/// Defines a handler for a request
/// </summary>
/// <typeparam name="TRequest">The type of request being handled</typeparam>
/// <typeparam name="TResponse">The type of response from the handler</typeparam>
public interface IRequestHandler<in TRequest, TResponse> : IRequestHandlerBase<TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles a request
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response from the request</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);

    async Task<TResponse> IRequestHandlerBase<TResponse>.Handle(object request, CancellationToken cancellationToken)
        => await Handle((TRequest)request, cancellationToken);
}

/// <summary>
/// Defines a handler for a request with a void (<see cref="Unit" />) response.
/// You do not need to register this interface explicitly with a container as it inherits from the base <see cref="IRequestHandler{TRequest, TResponse}" /> interface.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled</typeparam>
public interface IRequestHandler<in TRequest> : IRequestHandler<TRequest, Unit>
    where TRequest : IRequest<Unit>
{ }

/// <summary>
/// Base Request Handler that can be casted to, without knowing the request type
/// </summary>
/// <typeparam name="TResponse">The type of the response</typeparam>
public interface IRequestHandlerBase<TResponse>
{
    Task<TResponse> Handle(object request, CancellationToken cancellationToken);
}

/// <summary>
/// Wrapper class for a handler that asynchronously handles a request and does not return a response
/// </summary>
/// <typeparam name="TRequest">The type of request being handled</typeparam>
public abstract class AsyncRequestHandler<TRequest> : IRequestHandler<TRequest>
    where TRequest : IRequest
{
    async Task<Unit> IRequestHandler<TRequest, Unit>.Handle(TRequest request, CancellationToken cancellationToken)
    {
        await Handle(request, cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }

    /// <summary>
    /// Override in a derived class for the handler logic
    /// </summary>
    /// <param name="request">Request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Response</returns>
    protected abstract Task Handle(TRequest request, CancellationToken cancellationToken);
}
