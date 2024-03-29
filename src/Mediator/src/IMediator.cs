// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Mediator;

/// <summary>
/// Defines a mediator to encapsulate request/response and publishing interaction patterns
/// </summary>
public interface IMediator : ISender, IPublisher { }