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

public class RazorEmailRenderingService(IRazorViewRenderingService razorViewRenderingService, Func<IViewLocalizer> viewLocalizerFactory) : IEmailRenderingService
{
    public bool Supports(object model) => GetEmailViewAttribute(model) is not null;

    /// <inheritdoc />
    public async Task<(string Subject, string Body)> RenderAsync(
        object model,
        CultureInfo culture,
        string? subjectKey,
        object[]? subjectFormatParameters = null)
    {
        var emailViewAttribute = GetEmailViewAttribute(model)
            ?? throw new ArgumentNullException($"The Model {model.GetType().Name} is missing an {nameof(EmailViewAttribute)}.");

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
            // Get the view localizer and initialize it with the view context, so it knows where to take the resources from.
            var viewLocalizer = viewLocalizerFactory();
            (viewLocalizer as IViewContextAware)?.Contextualize(viewContext);

            subject = viewLocalizer.GetString(subjectKey, subjectFormatParameters ?? []) ?? subjectKey;
        }
    }

    private async Task<string> RenderAsync(object model, CultureInfo culture, Func<ActionContext, IView> findView, Action<ViewContext>? beforeRender = null)
    {
        var content = await razorViewRenderingService.RenderAsync(model, culture, findView, beforeRender);
        content = CssInliner.Inline(content);
        return content;
    }

    private static EmailViewAttribute? GetEmailViewAttribute(object model)
        => model.GetType().GetCustomAttribute<EmailViewAttribute>();
}
