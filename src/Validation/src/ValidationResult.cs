namespace Fusonic.Extensions.Validation
{
    public abstract class ValidationResult
    {
        public abstract bool IsValid { get; }

        public static ValidationResult Success() => new ValidationResult<object>();

        public static ValidationResult<TError> Error<TError>(TError error) where TError : notnull
            => new ValidationResult<TError>(error);
    }
}