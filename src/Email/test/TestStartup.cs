// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Email.Tests;
using Testcontainers.Mailpit;

[assembly: AssemblyFixture(typeof(TestStartup))]

namespace Fusonic.Extensions.Email.Tests;

/// <summary> Provides Mailpit as SMTP server for the tests. In CI it runs as a service, locally as a test container. </summary>
public sealed class TestStartup : IAsyncLifetime
{
    private MailpitContainer? container;

    public static string SmtpHost { get; private set; } = null!;
    public static int SmtpPort { get; private set; }
    public static Uri MailpitApiAddress { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        var host = Environment.GetEnvironmentVariable("Mailpit__Host");
        if (string.IsNullOrWhiteSpace(host))
        {
            container = new MailpitBuilder("axllent/mailpit:v1.34").Build();

            await container.StartAsync();
            SmtpHost = container.Hostname;
            SmtpPort = container.SmtpPort;
            MailpitApiAddress = new Uri(container.GetWebAddress());
        }
        else
        {
            SmtpHost = host;
            SmtpPort = 1025;
            MailpitApiAddress = new Uri($"http://{host}:8025");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (container != null)
            await container.DisposeAsync();
    }
}
