// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;

namespace Fusonic.Extensions.Email;

public interface IEmailRenderingService
{
    /// <summary>
    /// Gets if the rendering service supports the given model.
    /// </summary>
    bool Supports(object model);

    /// <summary>
    /// Renders a view based on the EmailViewAttribute which has to be present on the given model.
    /// </summary>
    /// <param name="model">Model to render. Must have the EmailViewAttribute.</param>
    /// <param name="culture">Culture to render the view in.</param>
    /// <param name="subjectKey">Subject key to get the subject of the email from the ViewLocalizer. If null, the SubjectKey from the EmailViewAttribute will be used.</param>
    /// <param name="subjectFormatParameters">String formatting parameters for the translated subject.</param>
    /// <returns>The email subject and the rendered body.</returns>
    Task<(string Subject, string Body)> RenderAsync(object model, CultureInfo culture, string? subjectKey, object[]? subjectFormatParameters = null);
}
