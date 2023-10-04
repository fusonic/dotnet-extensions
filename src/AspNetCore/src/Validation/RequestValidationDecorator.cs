// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Fusonic.Extensions.AspNetCore.Validation;

public class RequestValidationDecorator<TCommand, TResult>(IRequestHandler<TCommand, TResult> requestHandler, IObjectModelValidator objectModelValidator)
    : IRequestHandler<TCommand, TResult>
    where TCommand : IRequest<TResult>
{
    public Task<TResult> Handle(TCommand request, CancellationToken cancellationToken)
    {
        var actionContext = new ActionContext();
        objectModelValidator.Validate(actionContext, validationState: null, prefix: string.Empty, request);

        var modelState = actionContext.ModelState;
        if (modelState.IsValid)
            return requestHandler.Handle(request, cancellationToken);

        var errors = modelState.Where(s => s.Value != null)
                               .ToDictionary(
                                    kv => kv.Key,
                                    kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToList());

        throw new RequestValidationException(errors);
    }
}