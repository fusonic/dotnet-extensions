// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Fusonic.Extensions.AspNetCore.Blazor;

public class BlazorRenderingService(HtmlRenderer htmlRenderer) : IBlazorRenderingService
{
    public async Task<string> RenderComponent(IComponentModel model, CultureInfo culture)
    {
        var modelType = model.GetType();
        var componentType = model.ComponentType;

        var modelProperty = componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                         .FirstOrDefault(p => p.PropertyType == modelType)
            ?? throw new ArgumentNullException($"The Component {componentType.Name} is missing a property of type {modelType.Name}.");

        return await RenderComponent(componentType, culture, new() { [modelProperty.Name] = model }, beforeRender: null);
    }

    public Task<string> RenderComponent<T>(CultureInfo culture, Dictionary<string, object?>? dictionary = null) where T : IComponent
        => RenderComponentBase(typeof(T), culture, dictionary is null ? ParameterView.Empty : ParameterView.FromDictionary(dictionary), beforeRender: null);

    public Task<string> RenderComponent(Type componentType, CultureInfo culture, Dictionary<string, object?>? dictionary = null, Action? beforeRender = null)
        => RenderComponentBase(componentType, culture, dictionary is null ? ParameterView.Empty : ParameterView.FromDictionary(dictionary), beforeRender);

    private async Task<string> RenderComponentBase(Type componentType, CultureInfo culture, ParameterView parameters, Action? beforeRender)
        => await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var currentCulture = (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
            try
            {
                // The razor renderer takes the culture from the current thread culture.
                CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = culture;

                beforeRender?.Invoke();

                var output = await htmlRenderer.RenderComponentAsync(componentType, parameters);
                return output.ToHtmlString();
            }
            finally
            {
                (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture) = currentCulture;
            }
        });
}