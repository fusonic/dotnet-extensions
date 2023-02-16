// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.Email;

public class RazorEmailRenderingService : IEmailRenderingService
{
    private readonly IRazorViewEngine viewEngine;
    private readonly IServiceScope serviceScope;
    private readonly ITempDataProvider tempDataProvider;
    private readonly Func<IViewLocalizer> viewLocalizerFactory;

    public RazorEmailRenderingService(
        IRazorViewEngine viewEngine,
        IServiceScope serviceScope,
        ITempDataProvider tempDataProvider,
        Func<IViewLocalizer> viewLocalizerFactory)
    {
        this.viewEngine = viewEngine;
        this.serviceScope = serviceScope;
        this.tempDataProvider = tempDataProvider;
        this.viewLocalizerFactory = viewLocalizerFactory;
    }

    /// <inheritdoc />
    public async Task<(string Subject, string Body)> RenderAsync(
        object model,
        CultureInfo culture,
        string? subjectKey,
        object[]? subjectFormatParameters = null,
        Action<ViewContext>? beforeRender = null)
    {
        var modelType = model.GetType();
        var emailViewAttribute = modelType.GetCustomAttribute<EmailViewAttribute>();
        if (emailViewAttribute == null)
            throw new ArgumentNullException($"The Model {modelType.Name} is missing an EmailViewAttribute.");

        subjectKey ??= emailViewAttribute.SubjectKey;
        var subject = subjectKey;
        var body = await RenderAsync(model, culture, FindView, SetSubject);

        return (subject, body);

        IView FindView(ActionContext actionContext)
        {
            var viewResult = viewEngine.FindView(actionContext, emailViewAttribute!.ViewPath, false);
            if (!viewResult.Success)
                throw new FileNotFoundException($"The view {emailViewAttribute.ViewPath} could not be found. Searched locations: {string.Join(", ", viewResult.SearchedLocations)}");

            return viewResult.View;
        }

        void SetSubject(ViewContext viewContext)
        {
            beforeRender?.Invoke(viewContext);

            //Get the view localizer and initialize it with the view context, so it knows where to take the resources from.
            var viewLocalizer = viewLocalizerFactory();
            (viewLocalizer as IViewContextAware)?.Contextualize(viewContext);

            subject = viewLocalizer.GetString(subjectKey, subjectFormatParameters ?? Array.Empty<object>()) ?? subjectKey;
        }
    }

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
            var compiled = writer.ToString();
            return CssInliner.Inline(compiled);
        }
        finally
        {
            (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture) = currentCulture;
        }
    }
}
