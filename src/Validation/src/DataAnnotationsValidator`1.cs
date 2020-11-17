// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Fusonic.Extensions.Validation
{
    internal sealed class DataAnnotationsValidator<T> : IValidator<T> where T : class
    {
        public Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken)
        {
            return Task.FromResult((ValidationResult)DataAnnotationsValidator.Validate(instance));
        }
    }
}