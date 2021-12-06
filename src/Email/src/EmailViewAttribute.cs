// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Email;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class EmailViewAttribute : Attribute
{
    public EmailViewAttribute(string viewPath) => ViewPath = viewPath;
    public EmailViewAttribute(string viewPath, string subjectKey) => (ViewPath, SubjectKey) = (viewPath, subjectKey);

    /// <summary>
    /// Path to the view within the razor search paths.
    /// Example: A value of "Emails/FancyEmail" usually finds the View "Views/Emails/FancyEmail.cshtml"
    /// </summary>
    public string ViewPath { get; }

    /// <summary>
    /// Localizer-Key to use to get the subject. Defaults to "Subject".
    /// This key can be overwritten by explicitly specifying the subject in SendEmail.Command.
    /// </summary>
    public string SubjectKey { get; set; } = "Subject";
}
