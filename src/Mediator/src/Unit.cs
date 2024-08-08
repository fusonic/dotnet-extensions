// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Mediator;

/// <summary>
/// Represents a void type, since <code>void</code> is not a valid return type in C#.
/// </summary>
public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>, IComparable
{
    private static readonly Unit Instance = new();

    /// <summary>
    /// Default and only value of the <see cref="Unit"/> type.
    /// </summary>
    public static ref readonly Unit Value => ref Instance;

    /// <summary>
    /// Task from a <see cref="Unit"/> type.
    /// </summary>
    public static Task<Unit> Task { get; } = System.Threading.Tasks.Task.FromResult(Instance);

    /// <inheritdoc />
    public int CompareTo(Unit other) => 0;

    /// <inheritdoc />
    int IComparable.CompareTo(object? obj) => 0;

    /// <inheritdoc />
    public override int GetHashCode() => 0;

    /// <inheritdoc />
    public bool Equals(Unit other) => true;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Unit;

#pragma warning disable IDE0060 // Remove unused parameter
    public static bool operator ==(Unit first, Unit second) => true;
    public static bool operator !=(Unit first, Unit second) => false;
#pragma warning restore IDE0060 // Remove unused parameter

    public override string ToString() => "()";
}