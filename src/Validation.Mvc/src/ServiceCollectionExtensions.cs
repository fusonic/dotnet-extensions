using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.Validation.Mvc
{
    public static class ServiceCollectionExtensions
    {
        public static IMvcBuilder AddValidation(this IMvcBuilder mvcBuilder)
        {
            mvcBuilder.AddMvcOptions(o => o.Filters.Add<ObjectValidationExceptionFilter>());

            mvcBuilder.Services.AddSingleton<IObjectModelValidator, DataAnnotationsModelValidator>();

            // MVC has default conventions for API controllers. One of them is to return a 400 when model validation fails.
            // This prevents MVC from handling that case and registers our own behaviour.
            mvcBuilder.ConfigureApiBehaviorOptions(o => o.SuppressModelStateInvalidFilter = true);
            mvcBuilder.Services.AddSingleton<IApplicationModelProvider, ValidationApiBehaviorApplicationModelProvider>();

            return mvcBuilder;
        }
    }
}
