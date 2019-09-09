using System;

namespace Fusonic.Extensions.Validation
{
    public sealed class ObjectValidationException : Exception
    {
        public ObjectValidationException(object instance, ValidationResult result)
            : base("An error occurred validating the object.")
        {
            Instance = instance;
            Result = result;
        }

        public object Instance { get; }
        public ValidationResult Result { get; }
    }
}