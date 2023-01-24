// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql;

public class NpgsqlDatabasePerTestStoreOptions
{
    public NpgsqlDatabasePerTestStoreOptions()
    { }

    internal NpgsqlDatabasePerTestStoreOptions(NpgsqlDatabasePerTestStoreOptions copyFrom)
    {
        ConnectionString = copyFrom.ConnectionString;
        TemplateCreator = copyFrom.TemplateCreator;
        AlwaysCreateTemplate = copyFrom.AlwaysCreateTemplate;
    }

    /// <summary> Connection string to the template database. </summary>
    public string? ConnectionString { get; set; }

    /// <summary> Action to create the database template on first connect. Only gets executed once. If AlwaysCreateTemplate is set to false, the action only gets executed if the template database does not exist. </summary>
    public Func<string, Task>? TemplateCreator { get; set; }

    /// <summary> Ignores an existing template database and always recreates the template on the first run. Ignored, if TemplateCreator is null. </summary>
    public bool AlwaysCreateTemplate { get; set; }
}