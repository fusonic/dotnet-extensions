using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Fusonic.Extensions.Validation.Mvc
{
    internal class ValidationFailedActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.Result == null && !context.ModelState.IsValid)
            {
                var ex = (ObjectValidationException?)context.ModelState[DataAnnotationsModelValidator.ValidationResultKey]?.Errors.SingleOrDefault()?.Exception;

                if (ex is null)
                    context.Result = new BadRequestResult();
                else
                    context.Result = new ValidationFailedResult(ex.Result);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        { }
    }
}