// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Mediator.DependencyInjection;

internal interface IRequestHandlerWrapper
{
    Task<object> Handle(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}