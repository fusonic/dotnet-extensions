// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Fusonic.Extensions.Validation
{
    public interface IValidator<T>
        where T : notnull
    {
        Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken);
    }
}