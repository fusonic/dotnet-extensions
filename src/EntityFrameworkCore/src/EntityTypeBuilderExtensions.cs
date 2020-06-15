using System;
using System.Linq;
using System.Linq.Expressions;
using Fusonic.Extensions.Common.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fusonic.Extensions.EntityFrameworkCore
{
    public static class EntityTypeBuilderExtensions
    {
        public static IndexBuilder HasIndex<TEntity>(this EntityTypeBuilder<TEntity> builder, params Expression<Func<TEntity, object>>[] properties) where TEntity : class
            => builder.HasIndex(properties.Select(PropertyUtil.GetName).ToArray());
    }
}
