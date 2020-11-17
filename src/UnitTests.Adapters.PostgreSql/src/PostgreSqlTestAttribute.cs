// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System;
using Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;

namespace Fusonic.Extensions.UnitTests.Adapters.PostgreSql
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PostgreSqlTestAttribute : DatabaseProviderAttribute
    {
        /// <summary> If set to true all entity framework output will be logged to the test output. </summary>
        public bool EnableLogging { get; set; }
    }
}