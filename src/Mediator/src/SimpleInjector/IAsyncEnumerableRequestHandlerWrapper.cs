// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using SimpleInjector;

namespace Fusonic.Extensions.Mediator.SimpleInjector;

internal interface IAsyncEnumerableRequestHandlerWrapper
{
    IAsyncEnumerable<object> Handle(object request, Container container, CancellationToken cancellationToken);
}