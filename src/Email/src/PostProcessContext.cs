// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.AspNetCore.Blazor;

namespace Fusonic.Extensions.Email;

/// <summary>
/// A container that holds all relevant data to a rendered email.
/// </summary>
public abstract class PostProcessContext
{
    private protected PostProcessContext() { }

    /// <summary>
    /// The HTML content directly after rendering.
    /// </summary>
    public required string Html { get; init; }
}

/// <inheritdoc />
public sealed class RazorPostProcessContext : PostProcessContext
{
    /// <summary>
    /// The view model that was used to render the email.
    /// </summary>
    public required object ViewModel { get; init; }

    /// <summary>
    /// The <see cref="EmailViewAttribute"/> that the <see cref="ViewModel"/> is decorated with.
    /// </summary>
    public required EmailViewAttribute EmailViewAttribute { get; init; }
}

/// <inheritdoc />
public sealed class BlazorPostProcessContext : PostProcessContext
{
    /// <summary>
    /// The component model that was used to render the email.
    /// </summary>
    public required IComponentModel ComponentModel { get; init; }
}