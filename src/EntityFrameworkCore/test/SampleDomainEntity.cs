// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.EntityFrameworkCore.Abstractions;

namespace Fusonic.Extensions.EntityFrameworkCore.Tests;

public class SampleDomainEntity : IEntity<Guid>
{
    private SampleDomainEntity() { }

    public SampleDomainEntity(Guid id) => Id = id;

    public Guid Id { get; private set; }
    public DateTimeOffset Created { get; private set; } = DateTimeOffset.Now;
    public DateTimeOffset Modified { get; private set; } = DateTimeOffset.Now;
}