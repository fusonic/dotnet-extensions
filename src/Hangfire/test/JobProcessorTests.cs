// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Fusonic.Extensions.Common.Security;
using Fusonic.Extensions.Mediator;
using Hangfire;
using Hangfire.Server;
using Hangfire.Storage;
using NSubstitute;
using NSubstitute.ClearExtensions;
using Xunit;

namespace Fusonic.Extensions.Hangfire.Tests;

public class JobProcessorTests(TestFixture testFixture) : TestBase(testFixture)
{
    protected JobProcessor GetJobProcessor() => GetInstance<JobProcessor>();

    [Fact]
    public async Task CanProcessCommand()
    {
        await GetJobProcessor().ProcessAsync(new MediatorHandlerContext(new Command(), typeof(CommandHandler).AssemblyQualifiedName!), CreatePerformContext());
    }

    [Fact]
    public async Task CanProcessNotification()
    {
        await GetJobProcessor().ProcessAsync(new MediatorHandlerContext(new Notification(), typeof(NotificationHandler).AssemblyQualifiedName!), CreatePerformContext());
    }

    [Fact]
    public async Task CanProcessRequest()
    {
        await GetJobProcessor().ProcessAsync(new MediatorHandlerContext(new Request(), typeof(RequestHandler).AssemblyQualifiedName!), CreatePerformContext());
    }

    [Fact]
    public async Task RestoreCulture()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

        // Assertions are in the Command handler
        await GetJobProcessor().ProcessAsync(new MediatorHandlerContext(new CommandWithCulture(), typeof(CommandWithCulture.Handler).AssemblyQualifiedName!)
        {
            Culture = CultureInfo.CreateSpecificCulture("en-us"),
            UiCulture = CultureInfo.CreateSpecificCulture("de-at")
        }, CreatePerformContext());
    }

    [Fact]
    public async Task RestoresUser()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("test", "test")
        }));

        // Assertions are in the Command handler
        await GetJobProcessor().ProcessAsync(new MediatorHandlerContext(new CommandWithUser(), typeof(CommandWithUser.Handler).AssemblyQualifiedName!)
        {
            User = MediatorHandlerContext.HangfireUser.FromClaimsPrincipal(principal)
        }, CreatePerformContext());
    }

    [Fact]
    public async Task OutOfBandCommand_NoRecursion()
    {
        var jobClient = GetInstance<IBackgroundJobClient>();
        jobClient.ClearSubstitute();

        var handled = false;
        OutOfBandCommandHandler.Handled += (_, _) =>
        {
            handled = true;
            Assert.False(GetInstance<RuntimeOptions>().SkipOutOfBandDecorators);
        };

        await GetJobProcessor().ProcessAsync(new MediatorHandlerContext(new OutOfBandCommand(), typeof(OutOfBandCommandHandler).AssemblyQualifiedName!), CreatePerformContext());

        jobClient.DidNotReceiveWithAnyArgs().Enqueue<JobProcessor>(x => x.ProcessAsync(null!, null!));
        Assert.True(handled);
    }

    [Fact]
    public async Task OutOfBandNotification_NoRecursion()
    {
        var jobClient = GetInstance<IBackgroundJobClient>();
        jobClient.ClearSubstitute();

        var handled = false;
        OutOfBandNotificationHandler.Handled += (_, _) =>
        {
            handled = true;
            Assert.False(GetInstance<RuntimeOptions>().SkipOutOfBandDecorators);
        };

        await GetJobProcessor().ProcessAsync(new MediatorHandlerContext(new OutOfBandNotification(), typeof(OutOfBandNotificationHandler).AssemblyQualifiedName!), CreatePerformContext());

        jobClient.DidNotReceiveWithAnyArgs().Enqueue<JobProcessor>(x => x.ProcessAsync(null!, null!));
        Assert.True(handled);
    }

    private static PerformContext CreatePerformContext() => new(
        Substitute.For<JobStorage>(),
        Substitute.For<IStorageConnection>(),
        new BackgroundJob(Guid.NewGuid().ToString(), new JobData().Job, DateTime.UtcNow),
        Substitute.For<IJobCancellationToken>());

    public record CommandWithCulture : ICommand
    {
        public class Handler : IRequestHandler<CommandWithCulture>
        {
            public Task<Unit> Handle(CommandWithCulture request, CancellationToken cancellationToken)
            {
                CultureInfo.CurrentCulture.Should().Be(CultureInfo.CreateSpecificCulture("en-us"));
                CultureInfo.CurrentUICulture.Should().Be(CultureInfo.CreateSpecificCulture("de-at"));
                return Unit.Task;
            }
        }
    }

    public record CommandWithUser : ICommand
    {
        public class Handler(IUserAccessor userAccessor) : IRequestHandler<CommandWithUser>
        {
            public Task<Unit> Handle(CommandWithUser request, CancellationToken cancellationToken)
            {
                var user = userAccessor.User;
                user.Claims.Should().HaveCount(1);
                user.Claims.Should().Contain(c => c.Type == "test" && c.Value == "test");
                return Unit.Task;
            }
        }
    }
}
