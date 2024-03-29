// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Mediator.Tests.Notifications;

public record UserRegistered(string Username, UserRegistered.ApplicationState Status) : INotification
{
    public class ApplicationState(bool userWelcomed, bool freeTrialStarted)
    {
        public bool UserWelcomed => userWelcomed;

        public bool FreeTrialStarted => freeTrialStarted;

        public void WelcomeUser() => userWelcomed = true;
        public void StartFreeTrial() => freeTrialStarted = true;
    }
}