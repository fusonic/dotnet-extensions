using MediatR;

namespace Fusonic.Extensions.Abstractions
{
    /// <summary>
    /// Marks a query object. A query does not change data and is idempotent.
    /// </summary>
    /// <typeparam name="TResponse">The response object of the query.</typeparam>
    public interface IQuery<TResponse> : IRequest<TResponse>
    { }
}