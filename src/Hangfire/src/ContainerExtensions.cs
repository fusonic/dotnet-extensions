using System.Reflection;
using Fusonic.Extensions.Abstractions;
using MediatR;
using SimpleInjector;

namespace Fusonic.Extensions.Hangfire
{
    public static class ContainerExtensions
    {
        public static void RegisterOutOfBandDecorators(this Container container)
        {
            container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(OutOfBandRequestHandlerDecorator<>),
                c => c.ImplementationType.GetCustomAttribute<OutOfBandAttribute>() != null);

            container.RegisterDecorator(typeof(INotificationHandler<>), typeof(OutOfBandNotificationHandlerDecorator<>),
                c => c.ImplementationType.GetCustomAttribute<OutOfBandAttribute>() != null);
        }
    }
}