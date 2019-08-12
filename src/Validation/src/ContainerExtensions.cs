using System.Collections.Generic;
using System.Reflection;
using MediatR.Pipeline;
using SimpleInjector;

namespace Fusonic.Extensions.Validation
{
    public static class ContainerExtensions
    {
        public static void RegisterValidators(this Container container, IEnumerable<Assembly> assemblies)
        {
            var validators = container.GetTypesToRegister(typeof(IValidator<>),
                assemblies,
                new TypesToRegisterOptions
                {
                    IncludeGenericTypeDefinitions = true
                });

            container.Collection.Append(typeof(IValidator<>), typeof(DataAnnotationsValidator<>));
            container.Collection.Register(typeof(IValidator<>), validators);

            container.Collection.Append(typeof(IRequestPreProcessor<>), typeof(ValidationPreProcessor<>));
        }
    }
}