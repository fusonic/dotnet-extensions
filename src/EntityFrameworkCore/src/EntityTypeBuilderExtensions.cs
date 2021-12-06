// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Linq.Expressions;
using Fusonic.Extensions.Common.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fusonic.Extensions.EntityFrameworkCore;

public static class EntityTypeBuilderExtensions
{
    public static IndexBuilder HasIndex<TEntity>(this EntityTypeBuilder<TEntity> builder, params Expression<Func<TEntity, object>>[] properties) where TEntity : class
        => builder.HasIndex(properties.Select(PropertyUtil.GetName).ToArray());
}
