// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Common.Security;
using Fusonic.Extensions.Common.Transactions;
using Fusonic.Extensions.Mediator;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Fusonic.Extensions.Hangfire.Tests;

public class ServiceCollectionExtensionsTests : IDisposable
{
    private readonly ServiceCollection services = new();
    private ServiceProvider? serviceProvider;
    private AsyncServiceScope? scope;

    public ServiceCollectionExtensionsTests()
    {
        services.AddScoped<IRequestHandler<OutOfBandCommand, Unit>, OutOfBandCommandHandler>();
        services.AddScoped<IRequestHandler<Command, Unit>, CommandHandler>();
        services.AddScoped<INotificationHandler<OutOfBandNotification>, OutOfBandNotificationHandler>();
        services.AddScoped<INotificationHandler<Notification>, NotificationHandler>();
        services.AddSingleton(Substitute.For<IBackgroundJobClient>());
        services.AddSingleton(Substitute.For<IUserAccessor>());
    }

    private T GetInstance<T>() where T : notnull
    {
        serviceProvider ??= services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        scope ??= serviceProvider.CreateAsyncScope();
        return scope.Value.ServiceProvider.GetRequiredService<T>();
    }

    [Fact]
    public void OutOfBandRequestHandlerShouldBeResolved_IfOutOfBandAttributeHasBeenApplied()
    {
        services.AddOutOfBandDecorators();
        GetInstance<IRequestHandler<OutOfBandCommand, Unit>>().Should().BeOfType<OutOfBandRequestHandlerDecorator<OutOfBandCommand, ServiceProviderJobProcessor>>();
        GetInstance<IRequestHandler<Command, Unit>>().Should().BeOfType<CommandHandler>();
    }

    [Fact]
    public void JobProcessorMustBeResolvable()
    {
        services.AddOutOfBandDecorators();
        GetInstance<ServiceProviderJobProcessor>().Should().NotBeNull();
    }

    [Fact]
    public void OutOfBandRequestHandler_UsesCustomJobProcessor()
    {
        services.AddOutOfBandDecorators(options => options.UseJobProcessor<CustomJobProcessor>());
        GetInstance<IRequestHandler<OutOfBandCommand, Unit>>().Should().BeOfType<OutOfBandRequestHandlerDecorator<OutOfBandCommand, CustomJobProcessor>>();
        GetInstance<INotificationHandler<OutOfBandNotification>>().Should().BeOfType<OutOfBandNotificationHandlerDecorator<OutOfBandNotification, CustomJobProcessor>>();
    }

    [Fact]
    public void CustomJobProcessorMustBeResolvable()
    {
        services.AddOutOfBandDecorators(options => options.UseJobProcessor<CustomJobProcessor>());
        GetInstance<CustomJobProcessor>().Should().NotBeNull();
    }

    [Fact]
    public void NotificationHandlerShouldBeResolved_IfOutOfBandAttributeHasBeenApplied()
    {
        services.AddOutOfBandDecorators();
        GetInstance<INotificationHandler<OutOfBandNotification>>().Should().BeOfType<OutOfBandNotificationHandlerDecorator<OutOfBandNotification, ServiceProviderJobProcessor>>();
        GetInstance<INotificationHandler<Notification>>().Should().BeOfType<NotificationHandler>();
    }

    [Fact]
    public void UserAccessorDecoratorShouldBeApplied()
    {
        services.AddOutOfBandDecorators();
        GetInstance<IUserAccessor>().Should().BeOfType<HangfireUserAccessorDecorator>();
    }

    [Fact]
    public void UserAccessorDecorator_IsScoped()
    {
        services.AddOutOfBandDecorators();
        var accessor = GetInstance<IUserAccessor>();

        using var otherScope = serviceProvider!.CreateScope();
        otherScope.ServiceProvider.GetRequiredService<IUserAccessor>().Should().NotBeSameAs(accessor);
    }

    [Fact]
    public void ThrowsIfHandlersAreAlreadyDecorated()
    {
        AddTransactionalDecorators();
        var act = () => services.AddOutOfBandDecorators();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task JobProcessor_ProcessesOutOfBandNotification_WithinOuterDecorators()
    {
        services.AddScoped<INotificationHandler<OutOfBandNotification>, OutOfBandNotificationHandlerWithoutAttribute>();
        services.AddOutOfBandDecorators();
        AddTransactionalDecorators();

        var handled = false;
        var otherHandled = false;
        void OnHandled(object? sender, OutOfBandNotification e) => handled = true;
        void OnOtherHandled(object? sender, OutOfBandNotification e) => otherHandled = true;
        OutOfBandNotificationHandler.Handled += OnHandled;
        OutOfBandNotificationHandlerWithoutAttribute.Handled += OnOtherHandled;
        try
        {
            var context = new MediatorHandlerContext(new OutOfBandNotification(), typeof(OutOfBandNotificationHandler).AssemblyQualifiedName!);
            await GetInstance<ServiceProviderJobProcessor>().ProcessAsync(context, null!);
        }
        finally
        {
            OutOfBandNotificationHandler.Handled -= OnHandled;
            OutOfBandNotificationHandlerWithoutAttribute.Handled -= OnOtherHandled;
        }

        handled.Should().BeTrue();
        otherHandled.Should().BeFalse();
        GetInstance<IBackgroundJobClient>().ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task JobProcessor_ProcessesOutOfBandCommand()
    {
        services.AddOutOfBandDecorators();
        AddTransactionalDecorators();

        var handled = false;
        void OnHandled(object? sender, OutOfBandCommand e) => handled = true;
        OutOfBandCommandHandler.Handled += OnHandled;
        try
        {
            var context = new MediatorHandlerContext(new OutOfBandCommand(), typeof(OutOfBandCommandHandler).AssemblyQualifiedName!);
            await GetInstance<ServiceProviderJobProcessor>().ProcessAsync(context, null!);
        }
        finally
        {
            OutOfBandCommandHandler.Handled -= OnHandled;
        }

        handled.Should().BeTrue();
        GetInstance<IBackgroundJobClient>().ReceivedCalls().Should().BeEmpty();
    }

    private void AddTransactionalDecorators()
    {
        services.Decorate(typeof(IRequestHandler<,>), typeof(TransactionalRequestHandlerDecorator<,>));
        services.Decorate(typeof(INotificationHandler<>), typeof(TransactionalNotificationHandlerDecorator<>));
        services.AddSingleton<ITransactionScopeHandler, TransactionScopeHandler>();
    }

    private sealed class CustomJobProcessor(IServiceProvider serviceProvider) : ServiceProviderJobProcessor(serviceProvider);

    public void Dispose()
    {
        scope?.Dispose();
        serviceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}
