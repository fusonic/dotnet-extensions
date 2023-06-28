// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.AspNetCore.Razor;

public class RazorViewRenderingService : IRazorViewRenderingService
{
    private readonly IRazorViewEngine viewEngine;
    private readonly IServiceScope serviceScope;
    private readonly ITempDataProvider tempDataProvider;

    public RazorViewRenderingService(
        IRazorViewEngine viewEngine,
        IServiceScope serviceScope,
        ITempDataProvider tempDataProvider)
    {
        this.viewEngine = viewEngine;
        this.serviceScope = serviceScope;
        this.tempDataProvider = tempDataProvider;
    }

    /// <inheritdoc />
    public async Task<string> RenderAsync(object model, CultureInfo culture, Action<ViewContext>? beforeRender = null)
    {
        var modelType = model.GetType();
        var razorViewAttribute = modelType.GetCustomAttribute<RazorViewModelAttribute>()
                              ?? throw new ArgumentNullException($"The Model {modelType.Name} is missing a {nameof(RazorViewModelAttribute)}.");

        var body = await RenderAsync(model, culture, ctx => FindViewByPath(ctx, razorViewAttribute.ViewPath), beforeRender);
        return body;
    }

    /// <inheritdoc />
    public IView FindViewByPath(ActionContext actionContext, string viewPath)
    {
        var viewResult = viewEngine.FindView(actionContext, viewPath, isMainPage: false);
        if (!viewResult.Success)
            throw new FileNotFoundException($"The view {viewPath} could not be found. Searched locations: {string.Join(", ", viewResult.SearchedLocations)}");

        return viewResult.View;
    }

    /// <inheritdoc />
    public async Task<string> RenderAsync(object model, CultureInfo culture, Func<ActionContext, IView> findView, Action<ViewContext>? beforeRender = null)
    {
        var currentCulture = (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
        var httpContext = new DefaultHttpContext { RequestServices = serviceScope.ServiceProvider };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var view = findView(actionContext);

        var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model };

        try
        {
            //the razor renderer takes the culture from the current thread culture.
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = culture;

            await using var writer = new StringWriter();
            var viewContext = new ViewContext(
                actionContext,
                view,
                viewDictionary,
                new TempDataDictionary(actionContext.HttpContext, tempDataProvider),
                writer,
                new HtmlHelperOptions())
            {
                Html5DateRenderingMode = Html5DateRenderingMode.CurrentCulture
            };

            beforeRender?.Invoke(viewContext);

            await view.RenderAsync(viewContext);
            var content = writer.ToString();
            return content;
        }
        finally
        {
            (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture) = currentCulture;
        }
    }
}