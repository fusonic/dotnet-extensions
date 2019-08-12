using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Fusonic.Extensions.Validation.Mvc
{
    internal class ValidationFailedFilterConvention : IActionModelConvention
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
}