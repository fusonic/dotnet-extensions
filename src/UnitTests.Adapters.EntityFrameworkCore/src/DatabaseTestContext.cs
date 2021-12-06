// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;

internal static class DatabaseTestContext
{
    public static AsyncLocal<DatabaseProviderAttribute?> CurrentProviderAttribute = new AsyncLocal<DatabaseProviderAttribute?>();
}
