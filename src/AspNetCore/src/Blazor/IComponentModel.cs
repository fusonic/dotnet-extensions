// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.AspNetCore.Components;

namespace Fusonic.Extensions.AspNetCore.Blazor;

public interface IComponentModel<T> : IComponentModel where T : IComponent
{
    Type IComponentModel.ComponentType => typeof(T);
}

public interface IComponentModel
{
    Type ComponentType { get; }
}