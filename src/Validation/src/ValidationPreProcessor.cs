// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using MediatR.Pipeline;

namespace Fusonic.Extensions.Validation;

internal class ValidationPreProcessor<TRequest> : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> validators;

    public ValidationPreProcessor(IEnumerable<IValidator<TRequest>> validators)
    {
        this.validators = validators;
    }

    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
            {
                throw new ObjectValidationException(request, result);
            }
        }
    }
}
