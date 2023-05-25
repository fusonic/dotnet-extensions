// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Fusonic.Extensions.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fusonic.Extensions.EntityFrameworkCore;

public static class QueryableExtensions
{
    /// <summary> Ensure that the wanted entity is available </summary>
    /// <typeparam name="T">is a typeof an <see cref="IEntity"/></typeparam>
    /// <param name="entity">which is required to be available</param>
    /// <returns>the same entity back</returns>
    /// <exception cref="EntityNotFoundException">when the entity is not available</exception>
    public static T IsRequired<T>([NotNull] this T? entity)
        where T : class, IEntity
        => entity ?? throw new EntityNotFoundException(typeof(T));

    /// <summary> Ensure that the wanted entity is available </summary>
    /// <typeparam name="T">is a typeof an <see cref="IEntity"/></typeparam>
    /// <param name="entity">which is required to be available</param>
    /// <returns>the same entity back</returns>
    /// <exception cref="EntityNotFoundException">when the entity is not available</exception>
    public static Task<T> IsRequiredAsync<T>(this Task<T?> entity)
        where T : class, IEntity
        => entity.ContinueWith(t => t.Result.IsRequired(), TaskContinuationOptions.NotOnFaulted);

    /// <summary> Determines if the entity with the given ID exists. Throws a EntityNotFoundException if it does not. </summary>
    public static async Task IsRequiredAsync<T, TId>(this DbSet<T> dbSet, TId id, CancellationToken cancellationToken = default)
        where T : class, IEntity<TId>
        where TId : notnull
    {
        if (!await ExistsAsync(dbSet, id, cancellationToken))
            throw new EntityNotFoundException(typeof(T), id);
    }

    /// <summary> Determines if the query returns any result (AnyAsync). Throws a EntityNotFoundException if it does not. </summary>
    public static async Task IsRequiredAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        if (!await query.AnyAsync(cancellationToken))
            throw new EntityNotFoundException(typeof(T));
    }

    /// <summary> Check in the <see cref="DbSet{TEntity}"/> if the entity is available </summary>
    /// <typeparam name="T">is a typeof an <see cref="IEntity"/></typeparam>
    /// <typeparam name="TId">is a typeof an <see cref="IEntity"/> Id</typeparam>
    /// <exception cref="EntityNotFoundException">when the entity is not available</exception>
    public static async Task<T> FindRequiredAsync<T, TId>(this DbSet<T> dbSet, TId id, CancellationToken cancellationToken = default)
        where T : class, IEntity
        where TId : notnull
        => (await dbSet.FindAsync(new object[] { id }, cancellationToken)).IsRequired();

    /// <summary>
    /// Basically the same as SingleAsync(), but throws a EntityNotFoundException instead of an InvalidOperationException, when no result gets returned.
    /// This is useful for the first call in a query where the existence of an entity is checked.
    /// </summary>
    public static async Task<T> SingleRequiredAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            return await query.SingleAsync(predicate, cancellationToken);
        }
        catch (InvalidOperationException e) when (e.Message?.StartsWith("Sequence contains no elements", StringComparison.Ordinal) == true)
        {
            throw new EntityNotFoundException(typeof(T));
        }
    }

    /// <summary>
    /// Basically the same as SingleAsync(), but throws a EntityNotFoundException instead of an InvalidOperationException, when no result gets returned.
    /// This is useful for the first call in a query where the existence of an entity is checked.
    /// </summary>
    public static async Task<T> SingleRequiredAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        try
        {
            return await query.SingleAsync(cancellationToken);
        }
        catch (InvalidOperationException e) when (e.Message?.StartsWith("Sequence contains no elements", StringComparison.Ordinal) == true)
        {
            throw new EntityNotFoundException(typeof(T));
        }
    }

    /// <summary>
    /// Basically the same as SingleAsync(), but throws a EntityNotFoundException instead of an InvalidOperationException, when no result gets returned.
    /// This is useful for the first call in a query where the existence of an entity is checked.
    /// </summary>
    public static async Task<T> SingleRequiredAsync<T, TId>(this IQueryable<T> query, TId id, CancellationToken cancellationToken = default)
        where T : class, IEntity<TId>
        where TId : notnull
    {
        try
        {
            return await query.SingleAsync(e => Equals(e.Id, id), cancellationToken);
        }
        catch (InvalidOperationException e) when (e.Message?.StartsWith("Sequence contains no elements", StringComparison.Ordinal) == true)
        {
            throw new EntityNotFoundException(typeof(T), id);
        }
    }

    /// <summary> Determines if the entity with the given ID exists.</summary>
    public static async Task<bool> ExistsAsync<T, TId>(this DbSet<T> dbSet, TId id, CancellationToken cancellationToken = default)
        where T : class, IEntity<TId>
        where TId : notnull => await dbSet.AnyAsync(d => Equals(d.Id, id), cancellationToken);
}