// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using MediatR;

namespace Fusonic.Extensions.MediatR;

/// <summary>
/// Marks a command without a response. A command executes state changes.
/// </summary>
public interface ICommand : ICommand<Unit>, IRequest { }

/// <summary>
/// Marks a command with a response. A command executes state changes.
/// </summary>
/// <typeparam name="TResponse">The response object of the command, containing the execution result.</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse> { }
