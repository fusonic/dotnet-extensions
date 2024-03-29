// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.


namespace Fusonic.Extensions.Mediator.Tests.Notifications;

public class WelcomeUserEmailHandler : INotificationHandler<UserRegistered>
{
    public Task Handle(UserRegistered notification, CancellationToken cancellationToken)
    {
        notification.Status.WelcomeUser();
        return Task.CompletedTask;
    }
}
