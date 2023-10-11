// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore;

/// <summary>
/// This test store can be used for registration, if you need to support multiple test stores in your tests, eg. if you need to support multiple databases.
/// </summary>
public class AggregateTestStore : ITestStore
{
    private readonly List<ITestStore> testStores;

    public AggregateTestStore(params ITestStore[] testStores) => this.testStores = [.. testStores];
    public AggregateTestStore(ICollection<ITestStore> testStores) => this.testStores = [.. testStores];

    public void OnTestConstruction()
    {
        foreach (var ts in testStores)
        {
            ts.OnTestConstruction();
        }
    }

    public async Task OnTestEnd()
    {
        var exceptions = new List<Exception>();
        foreach (var ts in testStores)
        {
            try
            {
                await ts.OnTestEnd();
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
        }

        if (exceptions.Count != 0)
            throw new AggregateException(exceptions);
    }
}