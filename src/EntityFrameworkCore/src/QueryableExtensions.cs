﻿// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Fusonic.Extensions.EntityFrameworkCore.Domain;
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
    public static Task<T> IsRequiredAsync<T>([NotNull] this Task<T?> entity)
        where T : class, IEntity
        => entity.ContinueWith(t => t.Result.IsRequired(), TaskContinuationOptions.NotOnFaulted);

    /// <summary> Check in the <see cref="DbSet{TEntity}"/> if the entity is available </summary>
    /// <typeparam name="T">is a typeof an <see cref="IEntity"/></typeparam>
    /// <typeparam name="TId">is a typeof an <see cref="IEntity"/> Id</typeparam>
    /// <exception cref="EntityNotFoundException">when the entity is not available</exception>
    public static async Task<T> FindRequiredAsync<T, TId>(this DbSet<T> dbSet, TId id, CancellationToken cancellationToken = default)
        where TId : struct
        where T : class, IEntity<TId>
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
        catch (InvalidOperationException e) when (e.Message != null && e.Message.StartsWith("Sequence contains no elements", StringComparison.Ordinal))
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
        catch (InvalidOperationException e) when (e.Message != null && e.Message.StartsWith("Sequence contains no elements", StringComparison.Ordinal))
        {
            throw new EntityNotFoundException(typeof(T));
        }
    }

    /// <summary>
    /// Basically the same as SingleAsync(), but throws a EntityNotFoundException instead of an InvalidOperationException, when no result gets returned.
    /// This is useful for the first call in a query where the existence of an entity is checked.
    /// </summary>
    public static async Task<T> SingleRequiredAsync<T, TId>(this IQueryable<T> query, TId id, CancellationToken cancellationToken = default)
        where TId : struct
        where T : class, IEntity<TId>
    {
        try
        {
            return await query.SingleAsync(e => Equals(e.Id, id), cancellationToken);
        }
        catch (InvalidOperationException e) when (e.Message != null && e.Message.StartsWith("Sequence contains no elements", StringComparison.Ordinal))
        {
            throw new EntityNotFoundException(typeof(T), id);
        }
    }

    /// <summary> Determines if the entity with the given ID exists.</summary>
    public static async Task<bool> ExistsAsync<T, TId>(this DbSet<T> dbSet, TId id, CancellationToken cancellationToken = default)
        where TId : struct
        where T : class, IEntity<TId>
    {
        var exists = await dbSet.AnyAsync(d => Equals(d.Id, id), cancellationToken);
        return exists;
    }

    /// <summary> Determines if the entity with the given ID exists. Throws a EntityNotFoundException if it does not. </summary>
    public static async Task RequireAsync<T, TId>(this DbSet<T> dbSet, TId id, CancellationToken cancellationToken = default)
        where TId : struct
        where T : class, IEntity<TId>
    {
        if (!await ExistsAsync(dbSet, id, cancellationToken))
            throw new EntityNotFoundException(typeof(T), id);
    }
}