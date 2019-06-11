using MediatR;

namespace Fusonic.Extensions.Abstractions
{
    /// <summary>
    /// Marks a command without a response. A command changes business data.
    /// </summary>
    public interface ICommand : ICommand<Unit> { }

    /// <summary>
    /// Marks a command with a response. A command changes business data.
    /// </summary>
    /// <typeparam name="TResponse">The response object of the command, containig the execution result.</typeparam>
    public interface ICommand<TResponse> : IRequest<TResponse> { }
}