// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using MediatR;

namespace Fusonic.Extensions.MediatR
{
    /// <summary>
    /// Marks a query object. A query only responds with data. It never changes state and is consequently idempotent.
    /// </summary>
    /// <typeparam name="TResponse">The response object of the query.</typeparam>
    public interface IQuery<out TResponse> : IRequest<TResponse>
    { }
}