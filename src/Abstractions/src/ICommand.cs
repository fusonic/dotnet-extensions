using MediatR;

namespace Fusonic.Extensions.Abstractions
{
    /// <summary>
    /// Marks a command without a response. A command executes state changes.
    /// </summary>
    public interface ICommand : ICommand<Unit> { }

    /// <summary>
    /// Marks a command with a response. A command executes state changes.
    /// </summary>
    /// <typeparam name="TResponse">The response object of the command, containig the execution result.</typeparam>
    public interface ICommand<TResponse> : IRequest<TResponse> { }
}