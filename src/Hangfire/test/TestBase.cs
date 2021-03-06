﻿// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System;
using Fusonic.Extensions.Common.Security;
using Hangfire;
using MediatR;
using NSubstitute;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Fusonic.Extensions.Hangfire.Tests
{
    public abstract class TestBase : IDisposable
    {
        private Scope Scope { get; }
        protected IBackgroundJobClient JobClient { get; }
        protected IUserAccessor UserAccessor { get; }
        protected Container Container { get; } = new Container();

        protected TestBase()
        {
            Container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            Container.Options.ResolveUnregisteredConcreteTypes = false;

            Container.Register(typeof(IRequestHandler<,>), new[] { typeof(CommandHandler), typeof(RequestHandler), typeof(SyncRequestHandler), typeof(OutOfBandCommandHandler) });
            Container.Collection.Register(typeof(INotificationHandler<>), new[] { typeof(NotificationHandler), typeof(SyncNotificationHandler), typeof(OutOfBandNotificationHandler), typeof(OutOfBandNotificationHandlerWithoutAttribute) });
            Container.RegisterOutOfBandDecorators();

            JobClient = Substitute.For<IBackgroundJobClient>();
            Container.RegisterInstance(JobClient);
            UserAccessor = Substitute.For<IUserAccessor>();
            Container.RegisterInstance(UserAccessor);

            Scope = AsyncScopedLifestyle.BeginScope(Container);
        }

        public void Dispose() => Scope.Dispose();
    }
}
