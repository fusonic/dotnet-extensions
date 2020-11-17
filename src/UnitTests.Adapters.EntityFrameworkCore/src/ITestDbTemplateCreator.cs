// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore
{
    /// <summary> This interface can be implemented and passed to utils like pgtestutil (and others) so they're able to create a test template via simple command line options. </summary>
    public interface ITestDbTemplateCreator
    {
        void Create(string connectionString);
    }
}