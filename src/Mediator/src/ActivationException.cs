// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Mediator;

/// <summary>
/// Exception thrown when an activation of a type fails.
/// </summary>
public class ActivationException : Exception
{
    public ActivationException(string message) : base(message)
    { }

    public ActivationException(string message, Exception innerException) : base(message, innerException)
    { }
}