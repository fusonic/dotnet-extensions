// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Mediator;

public class MediatorOptions
{
    /// <summary>
    /// Controls whether <see cref="TransactionalRequestHandlerDecorator{TCommand, TResult}"/> and <see cref="TransactionalNotificationHandlerDecorator{TNotification}"/> are enabled. 
    /// Default is <c>true</c>.
    /// </summary>
    public bool EnableTransactionalDecorators { get; set; } = true;
}