// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Email.Tests.Models
{
    [EmailView("Emails/RenderTest")]
    public class RenderTestEmailViewModel
    {
        public string SomeField { get; set; } = null!;
    }
}