
// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore;

public interface ITestStore
{
    /// <summary> Gets called in the constructor of a DatabaseUnitTest </summary>
    void OnTestConstruction();

    /// <summary> Gets called in the `DisposeAsync` method of a `DatabaseUnitTest` </summary>
    Task OnTestEnd();
}