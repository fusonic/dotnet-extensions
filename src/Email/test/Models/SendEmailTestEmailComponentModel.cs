// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Email.Tests.Components.Emails;

namespace Fusonic.Extensions.Email.Tests.Models;

public class SendEmailTestEmailComponentModel : IComponentModel<SendEmailTest>
{
    public required string SomeField { get; set; }
}
