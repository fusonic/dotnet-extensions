// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.AspNetCore.Validation;

public class RequestValidationException : Exception
{
    public RequestValidationException(IDictionary<string, List<string>> errors) 
        : base($"""
                One or more validation errors occurred.
                {GetErrorMessage(errors)}
                """)
    {
        Errors = errors;
    }

    public RequestValidationException(Type requestType, IDictionary<string, List<string>> errors) 
        : base($"""
                One or more validation errors occurred.
                Request type: {requestType.FullName}.
                {GetErrorMessage(errors)}
                """)
    {
        RequestType = requestType;
        Errors = errors;
    }

    public IDictionary<string, List<string>> Errors { get; }
    public Type? RequestType { get; }

    private static string GetErrorMessage(IDictionary<string, List<string>> errors)
    {
        var messages = errors.SelectMany(err => err.Value.Select(e => $"{err.Key}: {e}"));
        var message = string.Join(Environment.NewLine, messages);
        return message;
    }
}