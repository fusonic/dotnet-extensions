// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Mediator;

/// <summary>
/// Marker interface to represent a request with a streaming response
/// </summary>
/// <typeparam name="TResponse">Response type</typeparam>
public interface IAsyncEnumerableRequest<out TResponse> { }
