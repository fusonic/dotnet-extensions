// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System;
using Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;

namespace Fusonic.Extensions.UnitTests.Adapters.InMemoryDatabase
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class InMemoryTestAttribute : DatabaseProviderAttribute
    { }
}
