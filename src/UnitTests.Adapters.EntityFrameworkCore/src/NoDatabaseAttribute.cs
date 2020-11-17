// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System;

namespace Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class NoDatabaseAttribute : DatabaseProviderAttribute
    { }
}