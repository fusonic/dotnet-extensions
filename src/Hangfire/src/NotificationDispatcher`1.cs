using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SimpleInjector;

namespace Fusonic.Extensions.Hangfire
{
    internal sealed class NotificationDispatcher<T> where T : INotification
    {
        private readonly Dictionary<Type, DependencyMetadata<INotificationHandler<T>>> metadataDictionary;

        public NotificationDispatcher(IList<DependencyMetadata<INotificationHandler<T>>> metadata)
        {
            metadataDictionary = metadata.ToDictionary(p => p.ImplementationType);
        }

        public Task Dispatch(T message, Type handlerType, CancellationToken cancellationToken)
        {
            var metadata = metadataDictionary[handlerType];
            var notificationHandler = metadata.GetInstance();
            return notificationHandler.Handle(message, cancellationToken);
        }
    }
}