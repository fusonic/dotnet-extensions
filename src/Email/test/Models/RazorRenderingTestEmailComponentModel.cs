// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Email.Tests.Components.Emails;

namespace Fusonic.Extensions.Email.Tests.Models;

public class RazorRenderingTestEmailComponentModel : IComponentModel<RazorRenderingTest>
{
    public DateTime Date { get; set; } = new(2019, 1, 9);
    public double Number { get; set; } = 1234567.89;
}
