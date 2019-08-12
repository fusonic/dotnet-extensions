using Microsoft.AspNetCore.Mvc;

namespace Fusonic.Extensions.Validation.Mvc
{
    internal class ValidationFailedResult : BadRequestObjectResult
    {
        public ValidationFailedResult(ValidationResult result)
            : base(result)
        {
        }
    }
}
