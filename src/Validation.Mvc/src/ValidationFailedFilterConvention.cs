// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Fusonic.Extensions.Validation.Mvc;

internal sealed class ValidationFailedFilterConvention : IActionModelConvention
{
    public void Apply(ActionModel action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        action.Filters.Add(new ValidationFailedActionFilter());
    }
}
