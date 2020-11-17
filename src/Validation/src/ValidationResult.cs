// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

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