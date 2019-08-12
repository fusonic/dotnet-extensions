using System.Collections.Generic;

namespace Fusonic.Extensions.Validation
{
    public class ValidationResult<TError> : ValidationResult
    {
        private readonly List<TError> errors = new List<TError>();

        public ValidationResult() { }

        public ValidationResult(TError error)
            => AddError(error);

        public override bool IsValid => errors.Count == 0;
        public IReadOnlyList<TError> Errors => errors.AsReadOnly();

        public void AddError(TError error)
            => errors.Add(error);
    }
}
