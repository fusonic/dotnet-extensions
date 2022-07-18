// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Validation;

public sealed class ModelValidationResult : ValidationResult
{
    private readonly Dictionary<string, List<string>> errors = new();
    public override bool IsValid => errors.Count == 0;

    public IReadOnlyDictionary<string, IReadOnlyList<string>> Errors
        => errors.ToDictionary(x => x.Key, x => (IReadOnlyList<string>)x.Value.AsReadOnly());

    public void AddError(string property, string error)
    {
        if (!errors.TryGetValue(property, out var errorMessages))
        {
            errorMessages = new List<string>();
            errors[property] = errorMessages;
        }

        errorMessages.Add(error);
    }
}
