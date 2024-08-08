// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Mediator.Tests.Requests;

public record EnableStateCommand(EnableStateCommand.ApplicationState State) : ICommand
{
    public class Handler : IRequestHandler<EnableStateCommand>
    {
        public Task<Unit> Handle(EnableStateCommand request, CancellationToken cancellationToken)
        {
            request.State.EnableStatus();

            return Task.FromResult(Unit.Value);
        }
    }
    public class ApplicationState(bool state)
    {
        public bool State => state;

        public void EnableStatus() => state = true;
    }
}

