// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Configuration;
using SimpleInjector;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql.Tests;

public class TestFixture : UnitTestFixture
{
    protected sealed override void RegisterCoreDependencies(Container container) => container.RegisterNpgsqlDbContext<TestDbContext>(Configuration.Bind);
}