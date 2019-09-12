using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Fusonic.Extensions.Validation.Mvc
{
    internal class DataAnnotationsModelValidator : IObjectModelValidator
    {
        private readonly IModelMetadataProvider modelMetadataProvider;
        public const string ValidationResultKey = "DataAnnotationsModelValidatorValidationResult";

        public DataAnnotationsModelValidator(IModelMetadataProvider modelMetadataProvider)
        {
            this.modelMetadataProvider = modelMetadataProvider;
        }

        public void Validate(ActionContext actionContext, ValidationStateDictionary validationState, string prefix, object model)
        {
            var result = DataAnnotationsValidator.Validate(model);
            if (!result.IsValid)
            {
                foreach (var (key, value) in result.Errors)
                {
                    foreach (var errorMessage in value)
                    {
                        actionContext.ModelState.TryAddModelError(key, errorMessage);
                    }
                }

                actionContext.ModelState.AddModelError(ValidationResultKey, new ObjectValidationException(model, result), modelMetadataProvider.GetMetadataForType(model.GetType()));
            }

            // set all unvalidated entries to valid
            foreach (var (key, entry) in actionContext.ModelState)
            {
                if (entry.ValidationState == ModelValidationState.Unvalidated)
                    entry.ValidationState = ModelValidationState.Valid;
            }
        }
    }
}