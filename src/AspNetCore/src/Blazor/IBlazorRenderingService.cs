// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace Fusonic.Extensions.AspNetCore.Blazor;

public interface IBlazorRenderingService
{
    Task<string> RenderComponent(IComponentModel model, CultureInfo culture);
    Task<string> RenderComponent<T>(CultureInfo culture, Dictionary<string, object?>? dictionary = null) where T : IComponent;
    Task<string> RenderComponent(Type componentType, CultureInfo culture, Dictionary<string, object?>? dictionary = null, Action? beforeRender = null);
}
