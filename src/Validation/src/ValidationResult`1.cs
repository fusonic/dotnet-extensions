// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Validation;

public class ValidationResult<TError> : ValidationResult where TError : notnull
{
    private readonly List<TError> errors = new List<TError>();

    public ValidationResult() { }

    public ValidationResult(TError error)
        => AddError(error);

    public override bool IsValid => errors.Count == 0;
    public IReadOnlyList<TError> Errors => errors.AsReadOnly();

    public void AddError(TError error)
        => errors.Add(error);
}
