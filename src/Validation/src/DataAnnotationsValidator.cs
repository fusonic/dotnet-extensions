// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Fusonic.Extensions.Validation;

public static class DataAnnotationsValidator
{
    public static ModelValidationResult Validate(object instance)
        => ValidateNode(instance, string.Empty, new ModelValidationResult(), new HashSet<object>());

    private static ModelValidationResult ValidateNode(object? instance, string path, ModelValidationResult result, HashSet<object> visited)
    {
        if (instance is null || !IsComplex(instance.GetType()) || visited.Contains(instance))
            return result;

        visited.Add(instance);

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

            if (value is IEnumerable enumerable && IsComplex(property.PropertyType) && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
            {
                ValidateCollection(enumerable, propertyPath, result, visited);
            }
            else
            {
                ValidateNode(value, propertyPath, result, visited);
            }
        }

        if (instance is IValidatableObject validatableObject)
        {
            foreach (var validationResult in validatableObject.Validate(new ValidationContext(instance)).Where(x => x != null))
            {
                if (validationResult.MemberNames.Any())
                {
                    foreach (var member in validationResult.MemberNames)
                    {
                        result.AddError(AppendProperty(path, member), validationResult.ErrorMessage ?? string.Empty);
                    }
                }
                else
                {
                    result.AddError(path, validationResult.ErrorMessage ?? string.Empty);
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

    private static void ValidateCollection(IEnumerable collection, string path, ModelValidationResult result, HashSet<object> visited)
    {
        if (collection is null)
            return;

        var i = 0;
        foreach (var item in collection)
        {
            ValidateNode(item, string.Concat(path, "[", i++, "]"), result, visited);
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
