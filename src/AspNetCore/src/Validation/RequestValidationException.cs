// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.AspNetCore.Validation;

public class RequestValidationException : Exception
{
    public IDictionary<string, List<string>> Errors { get; }
    public RequestValidationException(IDictionary<string, List<string>> errors) : base("One or more validation errors occurred.") => Errors = errors;
}