using Microsoft.AspNetCore.Mvc.Filters;

namespace Fusonic.Extensions.Validation.Mvc
{
    internal class ObjectValidationExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is ObjectValidationException exception)
            {
                context.ExceptionHandled = true;
                context.Result = new ValidationFailedResult(exception.Result);
            }
        }
    }
}