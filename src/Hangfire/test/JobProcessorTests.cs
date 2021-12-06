// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using System.Security.Claims;
using Fusonic.Extensions.Common.Security;
using Hangfire;
using Hangfire.Server;
using Hangfire.Storage;
using MediatR;
using NSubstitute;
using SimpleInjector;
using Xunit;

namespace Fusonic.Extensions.Hangfire.Tests;

public class HangfireJobProcessorTests : TestBase
{
    protected JobProcessor GetInstance() => Container.GetInstance<JobProcessor>();

    [Fact]
    public async Task CanProcessCommand()
    {
        await GetInstance().ProcessAsync(new MediatorHandlerContext(new Command(), typeof(CommandHandler).AssemblyQualifiedName!), CreatePerformContext());
    }

    [Fact]
    public async Task CanProcessNotification()
    {
        await GetInstance().ProcessAsync(new MediatorHandlerContext(new Notification(), typeof(NotificationHandler).AssemblyQualifiedName!), CreatePerformContext());
    }

    [Fact]
    public async Task CanProcessNotificationSync()
    {
        await GetInstance().ProcessAsync(new MediatorHandlerContext(new Notification(), typeof(SyncNotificationHandler).AssemblyQualifiedName!), CreatePerformContext());
    }

    [Fact]
    public async Task CanProcessRequest()
    {
        await GetInstance().ProcessAsync(new MediatorHandlerContext(new Request(), typeof(RequestHandler).AssemblyQualifiedName!), CreatePerformContext());
    }

    [Fact]
    public async Task CanProcessRequestSync()
    {
        await GetInstance().ProcessAsync(new MediatorHandlerContext(new SyncRequest(), typeof(SyncRequestHandler).AssemblyQualifiedName!), CreatePerformContext());
    }

    [Fact]
    public async Task RestoreCulture()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

        var specific = CultureInfo.CreateSpecificCulture("en-us");
        var specificUI = CultureInfo.CreateSpecificCulture("de-at");

        Container.Register<IRequestHandler<CommandWithCulture, Unit>>(() => new CommandHandlerWithCulture(() =>
        {
            Assert.Equal(specific, CultureInfo.CurrentCulture);
            Assert.Equal(specificUI, CultureInfo.CurrentUICulture);
        }));

        await GetInstance().ProcessAsync(new MediatorHandlerContext(new CommandWithCulture(), typeof(CommandHandlerWithCulture).AssemblyQualifiedName!)
        {
            Culture = specific,
            UiCulture = specificUI
        }, CreatePerformContext());
    }

    [Fact]
    public async Task RestoresUser()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
                new Claim("test", "test")
            }));

        Container.Register<IRequestHandler<CommandWithUser, Unit>>(() => new CommandHandlerWithUser((user) =>
        {
            Assert.NotSame(user, principal);
            Assert.Equal(user.Claims.Select(x => (x.Type, x.Value)), principal.Claims.Select(x => (x.Type, x.Value)));
        }, Container.GetInstance<IUserAccessor>()));

        await GetInstance().ProcessAsync(new MediatorHandlerContext(new CommandWithUser(), typeof(CommandHandlerWithUser).AssemblyQualifiedName!)
        {
            User = MediatorHandlerContext.HangfireUser.FromClaimsPrincipal(principal)
        }, CreatePerformContext());
    }

    [Fact]
    public async Task OutOfBandCommand_NoRecursion()
    {
        var handled = false;
        OutOfBandCommandHandler.Handled += (_, __) =>
        {
            handled = true;
            Assert.False(Container.GetInstance<RuntimeOptions>().SkipOutOfBandDecorators);
        };

        await GetInstance().ProcessAsync(new MediatorHandlerContext(new OutOfBandCommand(), typeof(OutOfBandCommandHandler).AssemblyQualifiedName!), CreatePerformContext());

        JobClient.DidNotReceiveWithAnyArgs().Enqueue<JobProcessor>(x => x.ProcessAsync(null!, null!));
        Assert.True(handled);
    }

    [Fact]
    public async Task OutOfBandNotification_NoRecursion()
    {
        var handled = false;
        OutOfBandNotificationHandler.Handled += (_, __) =>
        {
            handled = true;
            Assert.False(Container.GetInstance<RuntimeOptions>().SkipOutOfBandDecorators);
        };

        await GetInstance().ProcessAsync(new MediatorHandlerContext(new OutOfBandNotification(), typeof(OutOfBandNotificationHandler).AssemblyQualifiedName!), CreatePerformContext());

        JobClient.DidNotReceiveWithAnyArgs().Enqueue<JobProcessor>(x => x.ProcessAsync(null!, null!));
        Assert.True(handled);
    }

    private static PerformContext CreatePerformContext()
    {
        return new PerformContext(Substitute.For<JobStorage>(),
                Substitute.For<IStorageConnection>(),
                new BackgroundJob(Guid.NewGuid().ToString(), new JobData().Job, DateTime.UtcNow),
                Substitute.For<IJobCancellationToken>());
    }
}
