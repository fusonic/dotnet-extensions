using SimpleInjector;

namespace Fusonic.Extensions.Mediator.SimpleInjector;

internal interface INotificationHandlerWrapper
{
    Task Handle(object notification, Container container, CancellationToken cancellationToken);
}