// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Fusonic.Extensions.Common.Reflection;

namespace Fusonic.Extensions.Email;

public class EmailOptions
{
    /// <summary> Email-Address of the sender. </summary>
    public string? SenderAddress { get; set; }

    /// <summary> Name of the sender. </summary>
    public string? SenderName { get; set; }

    /// <summary> Enable SSL connection to the mail server. Default is true. </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary> SMTP Server address </summary>
    public string? SmtpServer { get; set; }

    /// <summary> SMTP Server port. Default is 465. </summary>
    public int SmtpPort { get; set; } = 465;

    /// <summary> SMTP Server username </summary>
    public string? SmtpUsername { get; set; }

    /// <summary> SMTP Server password </summary>
    public string? SmtpPassword { get; set; }

    /// <summary>
    /// If set, the subject is prefixed with that text. Use this if you want to distinguish mails in review environments.
    /// On production environments you want this to be null.
    /// Example: SubjectPrefix = "[MyBranch] "
    /// Result: "[MyBranch] Subject"
    /// </summary>
    public string? SubjectPrefix { get; set; }

    /// <summary>
    /// Instead of sending the email, it gets written to this directory. For debugging/development purposes only.
    /// If this property is set, all Smtp-Properties are ignored.
    /// </summary>
    public string? StoreInDirectory { get; set; }

    /// <summary>
    /// Path to the CSS files used to render the emails. Defaults to "[assemblyLocation]/wwwroot/assets/emails/email.css". Set to null if you don't want to use any CSS.
    /// Note: This is currently expected to be a file, not an embedded resource.
    /// </summary>
    public string? CssPath { get; set; } = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "wwwroot/assets/emails/email.css");

    /// <summary>
    /// Sets the default headers for all emails being sent.
    /// </summary>
    public IReadOnlyDictionary<string, string>? DefaultHeaders { get; set; }

    internal void Validate()
    {
        var errors = new List<string>();

        Required(x => x.SenderAddress);
        Required(x => x.SenderName);

        if (string.IsNullOrWhiteSpace(StoreInDirectory))
        {
            Required(x => x.SmtpServer);
            if (SmtpPort < 1)
                errors.Add("SMTP port is invalid.");
        }

        if (errors.Count > 0)
        {
            var message = string.Join(Environment.NewLine, errors);
            throw new ValidationException($"Email options are invalid.{Environment.NewLine}{message}");
        }

        void Required(Expression<Func<EmailOptions, string?>> expression)
        {
            if (string.IsNullOrWhiteSpace(expression.Compile()(this)))
                errors.Add($"{PropertyUtil.GetName(expression)} is required.");
        }
    }
}
