// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Npgsql;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql;

internal static class DatabaseHelper
{
    private static bool templateCreated;
    private static Exception? creationFailedException;
    private static readonly SemaphoreSlim CreationSync = new(1);

    /// <summary>
    /// Checks if a DB template exists. If it does not, the template gets created using the given action.
    /// Does not check if template is up to date if it does exist.
    /// Once the template is created, all future calls to this method just return without checking the template again.
    /// </summary>
    /// <param name="connectionString">Connection string to the template database.</param>
    /// <param name="createTemplate">Action to be executed to create the database.</param>
    /// <param name="alwaysCreateTemplate">When set to true, do not check if the template database exists, but always recreate the template on the first run. Defaults to false.</param>
    public static async Task EnsureCreated(string connectionString, Func<string, Task> createTemplate, bool alwaysCreateTemplate = false)
    {
        if (templateCreated)
            return;

        await CreationSync.WaitAsync().ConfigureAwait(false);
        try
        {
            if (templateCreated)
                return;

            if (creationFailedException != null)
                throw new InvalidOperationException("Template creation failed in another unit test. See InnerException for details.", creationFailedException);

            if (!alwaysCreateTemplate)
            {
                templateCreated = await PostgreSqlUtil.CheckDatabaseExists(connectionString).ConfigureAwait(false);
                if (templateCreated)
                    return;
            }

            try
            {
                await createTemplate(connectionString).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                creationFailedException = e;

                // Try to drop the template database. It is in a corrupt state and should be recreated in the next run.
                try
                {
                    NpgsqlConnection.ClearAllPools();
                    await PostgreSqlUtil.DropDatabase(connectionString).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    creationFailedException = new AggregateException(e, ex);
                    throw creationFailedException;
                }

                throw;
            }

            NpgsqlConnection.ClearAllPools();

            templateCreated = true;
        }
        finally
        {
            CreationSync.Release();
        }
    }
}