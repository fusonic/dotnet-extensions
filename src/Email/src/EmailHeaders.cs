// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using MimeKit;

namespace Fusonic.Extensions.Email;

public static class EmailHeaders
{
    public static readonly IReadOnlyDictionary<string, string> DiscourageAutoReplies = new Dictionary<string, string>
    {
        // https://www.rfc-editor.org/rfc/rfc3834 RFC3834 Section 2 §1
        [HeaderId.Precedence.ToHeaderName()] = "list",
        // https://www.rfc-editor.org/rfc/rfc3834 RFC3834 Section 2 §8
        [HeaderId.AutoSubmitted.ToHeaderName()] = "generated",
        // https://learn.microsoft.com/en-us/openspecs/exchange_server_protocols/ms-oxcmail/e489ffaf-19ed-4285-96d9-c31c42cab17f MS-OXCMAIL Section 2.2.3.2.14
        ["X-Auto-Response-Suppress"] = "All"
    }.AsReadOnly();
}
