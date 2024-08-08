using SimpleInjector;

namespace Fusonic.Extensions.Mediator.SimpleInjector;

internal interface IAsyncEnumerableRequestHandlerWrapper
{
    IAsyncEnumerable<object> Handle(object request, Container container, CancellationToken cancellationToken);
}