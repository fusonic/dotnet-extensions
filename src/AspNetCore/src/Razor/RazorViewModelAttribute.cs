// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.AspNetCore.Razor;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RazorViewModelAttribute(string viewPath) : Attribute
{
    /// <summary>
    /// Path to the view within the razor search paths.
    /// Example: A value of "Feature/FancyPage" usually finds the View "Views/Feature/FancyPage.cshtml"
    /// </summary>
    public string ViewPath => viewPath;
}