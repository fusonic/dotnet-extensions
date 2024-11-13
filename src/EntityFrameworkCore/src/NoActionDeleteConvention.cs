// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Fusonic.Extensions.EntityFrameworkCore;

/// <summary>
///     A more reasonable delete convention than <see cref="CascadeDeleteConvention"/>, that sets the delete behavior to <see cref="DeleteBehavior.NoAction" /> for required foreign keys
///     and <see cref="DeleteBehavior.ClientSetNull" /> for optional ones.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class NoActionDeleteConvention
    : IForeignKeyAddedConvention,
      IForeignKeyRequirednessChangedConvention
{
    public virtual void ProcessForeignKeyAdded(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext<IConventionForeignKeyBuilder> context
    )
    {
        var newRelationshipBuilder = relationshipBuilder.OnDelete(
            GetTargetDeleteBehavior(relationshipBuilder.Metadata)
        );

        if (newRelationshipBuilder != null)
        {
            context.StopProcessingIfChanged(newRelationshipBuilder);
        }
    }

    public virtual void ProcessForeignKeyRequirednessChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext<bool?> context
    )
    {
        var newRelationshipBuilder = relationshipBuilder.OnDelete(
            GetTargetDeleteBehavior(relationshipBuilder.Metadata)
        );

        if (newRelationshipBuilder != null)
        {
            context.StopProcessingIfChanged(newRelationshipBuilder.Metadata.IsRequired);
        }
    }

    protected virtual DeleteBehavior GetTargetDeleteBehavior(IConventionForeignKey foreignKey)
        => foreignKey.IsRequired ? DeleteBehavior.NoAction : DeleteBehavior.ClientSetNull;
}
