// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using System.Reflection;
using Fusonic.Extensions.AspNetCore.Blazor;
using Microsoft.Extensions.Localization;

namespace Fusonic.Extensions.Email;

public class BlazorEmailRenderingService(IBlazorRenderingService blazorRenderingService, IStringLocalizerFactory stringLocalizerFactory) : IEmailRenderingService
{
    private const string DefaultSubjectKey = "Subject";

    public bool Supports(object model) => model is IComponentModel;

    /// <inheritdoc />
    public async Task<(string Subject, string Body)> RenderAsync(
        object model,
        CultureInfo culture,
        string? subjectKey,
        object[]? subjectFormatParameters = null)
    {
        var modelType = model.GetType();

        if (model is not IComponentModel componentModel)
            throw new ArgumentException($"The type {modelType.Name} does not implement {nameof(IComponentModel)}.", nameof(model));

        var componentType = componentModel.ComponentType;
        var modelProperty = componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                         .FirstOrDefault(p => p.PropertyType == modelType)
            ?? throw new ArgumentNullException(nameof(model), $"The Component {componentType.Name} is missing a property of type {modelType.Name}.");

        subjectKey ??= (componentModel as IProvideEmailSubject)?.SubjectKey ?? DefaultSubjectKey;
        var subject = subjectKey;

        var body = await RenderAsync(
            componentType,
            modelProperty.Name,
            componentModel,
            culture,
            beforeRender: SetSubject);

        return (subject, body);

        void SetSubject()
        {
            var subjectLocalization = stringLocalizerFactory.Create(componentType).GetString(subjectKey, subjectFormatParameters ?? []);
            subject = subjectLocalization.ResourceNotFound ? subjectKey : subjectLocalization.Value;
        }
    }

    private async Task<string> RenderAsync(Type componentType, string modelPropertyName, IComponentModel model, CultureInfo culture, Action beforeRender)
    {
        var content = await blazorRenderingService.RenderComponent(componentType, culture, new() { [modelPropertyName] = model }, beforeRender);
        content = CssInliner.Inline(content);
        return content;
    }
}
