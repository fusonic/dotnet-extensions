// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Mediator;

/// <summary>
/// Defines a handler for a stream request using IAsyncEnumerable as return type.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled</typeparam>
/// <typeparam name="TResponse">The type of response from the handler</typeparam>
public interface IAsyncEnumerableRequestHandler<in TRequest, out TResponse>
    where TRequest : IAsyncEnumerableRequest<TResponse>
{
    /// <summary>
    /// Handles a stream request with IAsyncEnumerable as return type.
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response from the request</returns>
    IAsyncEnumerable<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
