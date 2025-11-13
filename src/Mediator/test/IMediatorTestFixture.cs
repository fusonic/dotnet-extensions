// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Mediator.Tests;

public interface IMediatorTestFixture
{
    bool EnableTransactionalDecorators { get; }
}