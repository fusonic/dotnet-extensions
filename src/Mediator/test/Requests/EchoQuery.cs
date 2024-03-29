// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.


namespace Fusonic.Extensions.Mediator.Tests.Requests;

public record EchoQuery(string Echo) : IQuery<EchoQuery.Result>
{
    public record Result(string Value);

    public class Handler : IRequestHandler<EchoQuery, Result>
    {
        public Task<Result> Handle(EchoQuery request, CancellationToken cancellationToken)
            => Task.FromResult(new Result(request.Echo));
    }
}
