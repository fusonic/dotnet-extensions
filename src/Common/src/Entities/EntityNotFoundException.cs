// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.


namespace Fusonic.Extensions.Common.Entities;

public sealed class EntityNotFoundException : Exception
{
    public Type? EntityType { get; private set; }
    public object? EntityId { get; private set; }

    public EntityNotFoundException() : base("Could not find entity.") { }

    public EntityNotFoundException(string message) : base(message) { }

    public EntityNotFoundException(Type entityType, string message) : this(message) => EntityType = entityType;

    public EntityNotFoundException(Type entityType) : this(entityType, $"Could not find entity of type '{entityType.Name}'.") { }

    public EntityNotFoundException(Type entityType, object entityId, string message) : this(entityType, message) => EntityId = entityId;

    public EntityNotFoundException(Type entityType, object entityId) : this(entityType, entityId, $"Could not find entity of type '{entityType.Name}' with id {entityId}.") { }
}