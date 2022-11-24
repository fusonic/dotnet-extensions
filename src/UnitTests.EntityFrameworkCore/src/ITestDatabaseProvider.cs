// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore;

public interface ITestDatabaseProvider
{
    /// <summary>
    /// Gets the name of the test database for one test. Can be different for each test.
    /// </summary>
    string TestDbName { get; }

    /// <summary>
    /// Create the database for the currently running test.
    /// </summary>
    /// <param name="dbContext">The DbContext with the connection pointing to the test database. The test database might not exist yet at this point.</param>
    void CreateDatabase(DbContext dbContext);

    /// <summary>
    /// Drops the previously created test database. Can be called even if no database was created before.
    /// </summary>
    void DropDatabase(DbContext dbContext);
}