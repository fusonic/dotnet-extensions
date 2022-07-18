// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.AspNetCore.Mvc.Filters;

namespace Fusonic.Extensions.Validation.Mvc;

internal sealed class ObjectValidationExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ObjectValidationException exception)
        {
            context.ExceptionHandled = true;
            context.Result = new ValidationFailedResult(exception.Result);
        }
    }
}
