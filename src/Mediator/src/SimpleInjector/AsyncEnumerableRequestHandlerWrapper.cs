using System.Runtime.CompilerServices;
using SimpleInjector;

namespace Fusonic.Extensions.Mediator.SimpleInjector;

internal sealed class AsyncEnumerableRequestHandlerWrapper<TRequest, TResponse> : IAsyncEnumerableRequestHandlerWrapper
    where TRequest : IAsyncEnumerableRequest<TResponse>
{
    public async IAsyncEnumerable<object> Handle(object request, Container container, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var handlers = container.GetInstance<IAsyncEnumerableRequestHandler<TRequest, TResponse>>();
        await foreach (var item in handlers.Handle((TRequest)request, cancellationToken))
        {
            yield return item!;
        }
    }
}