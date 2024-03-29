// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.


namespace Fusonic.Extensions.Mediator.Tests.Requests;

public record MultiplyCommand(int Left, int Right) : ICommand<MultiplyCommand.Result>
{
    public record Result(int Value);

    public class Handler : IRequestHandler<MultiplyCommand, Result>
    {
        public Task<Result> Handle(MultiplyCommand request, CancellationToken cancellationToken)
            => Task.FromResult(new Result(request.Left * request.Right));
    }
}
