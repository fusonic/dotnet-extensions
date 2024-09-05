// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using SimpleInjector;

namespace Fusonic.Extensions.Mediator.SimpleInjector;

internal interface INotificationHandlerWrapper
{
    Task Handle(object notification, Container container, CancellationToken cancellationToken);
}