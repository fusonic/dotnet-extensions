// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.AspNetCore.Validation;

public class RequestValidationException(IDictionary<string, List<string>> errors) : Exception("One or more validation errors occurred.")
{
    public IDictionary<string, List<string>> Errors { get; } = errors;
}