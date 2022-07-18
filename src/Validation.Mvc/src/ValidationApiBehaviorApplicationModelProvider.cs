// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Fusonic.Extensions.Validation.Mvc;

internal sealed class ValidationApiBehaviorApplicationModelProvider : IApplicationModelProvider
{
    private readonly ValidationFailedFilterConvention convention;

    public ValidationApiBehaviorApplicationModelProvider()
    {
        convention = new ValidationFailedFilterConvention();
    }

    public int Order => 0;

    public void OnProvidersExecuting(ApplicationModelProviderContext context)
    {
        foreach (var controller in context.Result.Controllers)
        {
            if (!IsApiController(controller))
                continue;

            foreach (var action in controller.Actions)
            {
                convention.Apply(action);
            }
        }
    }

    public void OnProvidersExecuted(ApplicationModelProviderContext context)
    {
    }

    private static bool IsApiController(ControllerModel controller)
    {
        if (controller.Attributes.OfType<ApiControllerAttribute>().Any())
        {
            return true;
        }

        return controller.ControllerType.Assembly.GetCustomAttribute<ApiControllerAttribute>() != null;
    }
}
