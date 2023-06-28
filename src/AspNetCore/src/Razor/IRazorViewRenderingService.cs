// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Fusonic.Extensions.AspNetCore.Razor;

public interface IRazorViewRenderingService
{
    /// <summary>
    /// Renders a view based on the RazorViewAttribute which has to be present on the given model.
    /// </summary>
    /// <param name="model">Model to render. Must have the RazorViewAttribute.</param>
    /// <param name="culture">Culture to render the view in.</param>
    /// <param name="beforeRender">A chance to modify or use the view context before rendering.</param>
    /// <returns>The rendered view body.</returns>
    Task<string> RenderAsync(object model, CultureInfo culture, Action<ViewContext>? beforeRender = null);

    /// <summary>
    /// Renders a view based on the provided view.
    /// </summary>
    /// <param name="model">Model to render. Must have the EmailViewAttribute.</param>
    /// <param name="culture">Culture to render the view in.</param>
    /// <param name="findView">Returns the view that should be rendered.</param>
    /// <param name="beforeRender">A chance to modify or use the view context before rendering.</param>
    /// <returns>The rendered view body.</returns>
    Task<string> RenderAsync(object model, CultureInfo culture, Func<ActionContext, IView> findView, Action<ViewContext>? beforeRender = null);

    /// <summary> Looks up the view using the given path. </summary>
    IView FindViewByPath(ActionContext actionContext, string viewPath);
}