// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.AspNetCore.Mvc;

namespace Fusonic.Extensions.Validation.Mvc;

internal sealed class ValidationFailedResult : BadRequestObjectResult
{
    public ValidationFailedResult(ValidationResult result)
        : base(result)
    {
    }
}
