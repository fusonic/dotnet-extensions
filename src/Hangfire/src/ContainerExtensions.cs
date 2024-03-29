// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Reflection;
using Fusonic.Extensions.Common.Security;
using SimpleInjector;
using Fusonic.Extensions.Mediator;

namespace Fusonic.Extensions.Hangfire;

public static class ContainerExtensions
{
    public static void RegisterOutOfBandDecorators(this Container container, Action<OutOfBandOptions>? configure = null)
    {
        var options = new OutOfBandOptions();
        configure?.Invoke(options);
        var jobProcessorType = options.JobProcessorType;

        container.Register(jobProcessorType, jobProcessorType, Lifestyle.Scoped);
        container.Register<RuntimeOptions>(Lifestyle.Scoped);
        container.Register(typeof(NotificationDispatcher<>), typeof(NotificationDispatcher<>), Lifestyle.Singleton);

        var requestHandlerDecoratorType = CreatePartiallyClosedGenericType(typeof(OutOfBandRequestHandlerDecorator<,>));
        container.RegisterDecorator(typeof(IRequestHandler<,>), requestHandlerDecoratorType, Lifestyle.Scoped,
            c => c.ImplementationType.GetCustomAttribute<OutOfBandAttribute>() != null);

        var notificationHandlerGenericType = CreatePartiallyClosedGenericType(typeof(OutOfBandNotificationHandlerDecorator<,>));
        container.RegisterDecorator(typeof(INotificationHandler<>), notificationHandlerGenericType, Lifestyle.Scoped,
            c => c.ImplementationType.GetCustomAttribute<OutOfBandAttribute>() != null);

        container.RegisterDecorator<IUserAccessor, HangfireUserAccessorDecorator>(Lifestyle.Scoped);

        Type CreatePartiallyClosedGenericType(Type openGenericDecorator)
        {
            var genericArguments = openGenericDecorator.GetGenericArguments();
            genericArguments[1] = jobProcessorType;
            return openGenericDecorator.MakeGenericType(genericArguments);
        }
    }

    public sealed class OutOfBandOptions
    {
        internal Type JobProcessorType { get; private set; } = typeof(JobProcessor);

        public void UseJobProcessor<TJobProcessor>()
            where TJobProcessor : class, IJobProcessor
            => JobProcessorType = typeof(TJobProcessor);
    }
}
