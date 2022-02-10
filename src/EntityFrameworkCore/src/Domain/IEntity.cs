// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.EntityFrameworkCore.Domain;

public interface IEntity
{ }

public interface IEntity<out T> : IEntity
    where T : struct
{
    T Id { get; }
}