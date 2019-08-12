using System;
using System.Runtime.Serialization;

namespace Fusonic.Extensions.Validation
{
    [Serializable]
    public sealed class ObjectValidationException : Exception
    {
        public ObjectValidationException(object instance, ValidationResult result)
            : base("An error occurred validation the object")
        {
            Instance = instance;
            Result = result;
        }

        private ObjectValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public object Instance { get; }
        public ValidationResult Result { get; }
    }
}