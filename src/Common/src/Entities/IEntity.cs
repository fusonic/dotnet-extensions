// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Common.Entities;

public interface IEntity
{ }

public interface IEntity<out T> : IEntity
    where T : notnull
{
    T Id { get; }
}