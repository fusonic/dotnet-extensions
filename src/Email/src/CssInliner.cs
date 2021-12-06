// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Text;

namespace Fusonic.Extensions.Email;

public class CssInliner
{
    public static string EmailCssContent { get; private set; } = null!;

    public CssInliner(EmailOptions options)
    {
        if (EmailCssContent == null && options.CssPath != null)
            EmailCssContent = File.ReadAllText(options.CssPath, Encoding.UTF8);
    }

    public static string Inline(string source)
    {
        using var pm = new PreMailer.Net.PreMailer(source);
        var html = pm.MoveCssInline(css: EmailCssContent, removeComments: true, stripIdAndClassAttributes: false, removeStyleElements: false).Html;
        return html;
    }
}
