using Newtonsoft.Json;

namespace Fusonic.Extensions.Validation
{
    public abstract class ValidationResult
    {
        [JsonIgnore]
        public abstract bool IsValid { get; }

        public static ValidationResult Success() => new ValidationResult<object>();
        public static ValidationResult Error<TError>(TError error) => new ValidationResult<TError>(error);
    }
}
