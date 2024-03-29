// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Runtime.CompilerServices;

namespace Fusonic.Extensions.Mediator.Tests.Requests;

public record ExportDataQuery : IAsyncEnumerableRequest<ExportDataQuery.Result>
{
    public record Result(int Id, string Data);

    public class Handler : IAsyncEnumerableRequestHandler<ExportDataQuery, Result>
    {
        public async IAsyncEnumerable<Result> Handle(ExportDataQuery request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            List<Result> data = [new Result(1, "1"), new Result(2, "2"), new Result(3, "3"), new Result(4, "4")];
            foreach (var item in data)
            {
                await Task.Delay(100, cancellationToken);
                yield return item;
            }
        }
    }
}
