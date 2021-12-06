// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Hosting.TimedHostedService;

public class TimedHostedServiceSettings
{
    /// <summary> Interval in seconds in which the sync gets started. Defaults to 900s / 15min. </summary>
    public int Interval { get; set; } = 15 * 60;

    /// <summary> Url that gets notified that the service is still alive. </summary>
    public Uri? WatchdogUri { get; set; }
}
