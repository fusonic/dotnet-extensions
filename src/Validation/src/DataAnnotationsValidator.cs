using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Fusonic.Extensions.Validation
{
    public static class DataAnnotationsValidator
    {
        public static ModelValidationResult Validate(object instance)
            => ValidateNode(instance, string.Empty, new ModelValidationResult());

        private static ModelValidationResult ValidateNode(object instance, string path, ModelValidationResult result)
        {
            if (instance is null)
                return result;

            foreach (var property in instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propertyPath = AppendProperty(path, property.Name);
                var value = property.GetValue(instance);

                foreach (var attribute in property.GetCustomAttributes<ValidationAttribute>(true))
                {
                    if (!attribute.IsValid(value))
                    {
                        result.AddError(propertyPath, attribute.FormatErrorMessage(property.Name));
                    }
                }

                if (typeof(ICollection).IsAssignableFrom(property.PropertyType))
                {
                    ValidateCollection((ICollection)value, propertyPath, result);
                }
                else if (IsComplex(property.PropertyType))
                {
                    ValidateNode(value, propertyPath, result);
                }
            }

            if (instance is IValidatableObject validatableObject)
            {
                var results = validatableObject.Validate(new ValidationContext(instance)).Where(x => x != null);
                foreach (var validationResult in results)
                {
                    if (validationResult.MemberNames.Any())
                    {
                        foreach (var member in validationResult.MemberNames)
                        {
                            result.AddError(AppendProperty(path, member), validationResult.ErrorMessage);
                        }
                    }
                    else
                    {
                        result.AddError(path, validationResult.ErrorMessage);
                    }
                }
            }

            return result;
        }

        private static string AppendProperty(string path, string property)
        {
            if (string.IsNullOrEmpty(path))
                return property;

            return string.Concat(path, ".", property);
        }

        private static void ValidateCollection(ICollection collection, string path, ModelValidationResult result)
        {
            if (collection is null)
                return;

            var i = 0;
            foreach (var item in collection)
            {
                ValidateNode(item, string.Concat(path, "[", i++, "]"), result);
            }
        }

        private static bool IsComplex(Type type)
        {
            return !(type.IsPrimitive
                     || type.IsEnum
                     || type.IsValueType
                     || type == typeof(string));
        }
    }
}