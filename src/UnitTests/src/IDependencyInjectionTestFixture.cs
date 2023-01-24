// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.UnitTests;

public interface IDependencyInjectionTestFixture
{
    object BeginScope();
    object GetInstance(object scope, Type type);
    T GetInstance<T>(object scope) where T : class;
}