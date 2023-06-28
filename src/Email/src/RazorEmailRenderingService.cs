// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using System.Reflection;
using Fusonic.Extensions.AspNetCore.Razor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Fusonic.Extensions.Email;

public class RazorEmailRenderingService : IEmailRenderingService
{
    private readonly IRazorViewRenderingService razorViewRenderingService;
    private readonly Func<IViewLocalizer> viewLocalizerFactory;

    public RazorEmailRenderingService(IRazorViewRenderingService razorViewRenderingService, Func<IViewLocalizer> viewLocalizerFactory)
    {
        this.razorViewRenderingService = razorViewRenderingService;
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
        var emailViewAttribute = modelType.GetCustomAttribute<EmailViewAttribute>()
                              ?? throw new ArgumentNullException($"The Model {modelType.Name} is missing an {nameof(EmailViewAttribute)}.");

        subjectKey ??= emailViewAttribute.SubjectKey;
        var subject = subjectKey;

        var body = await RenderAsync(
            model,
            culture,
            findView: ctx => razorViewRenderingService.FindViewByPath(ctx, emailViewAttribute.ViewPath),
            beforeRender: SetSubject);

        return (subject, body);

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
        var content = await razorViewRenderingService.RenderAsync(model, culture, findView, beforeRender);
        content = CssInliner.Inline(content);
        return content;
    }
}
