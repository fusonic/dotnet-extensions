using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Fusonic.Extensions.Email
{
    public interface IEmailRenderingService
    {
        /// <summary>
        /// Renders a view based on the EmailViewAttribute which has to be present on the given model.
        /// </summary>
        /// <param name="model">Model to render. Must have the EmailViewAttribute.</param>
        /// <param name="culture">Culture to render the model in.</param>
        /// <param name="subjectKey">Subject key to get the subject of the email from the ViewLocalizer. If null, the SubjectKey from the EmailViewAttribute will be used.</param>
        /// <param name="beforeRender">A chance to modify or use the view context before rendering.</param>
        /// <returns>The email subject and the rendered body.</returns>
        Task<(string Subject, string Body)> RenderAsync(object model, CultureInfo culture, string? subjectKey, Action<ViewContext>? beforeRender = null);

        /// <summary>
        /// Renders a view based on the provided view path.
        /// </summary>
        /// <param name="model">Model to render. Must have the EmailViewAttribute.</param>
        /// <param name="culture">Culture to render the model in.</param>
        /// <param name="findView">Returns the view that should be rendered.</param>
        /// <param name="beforeRender">A chance to modify or use the view context before rendering.</param>
        /// <returns>The rendered email body.</returns>
        Task<string> RenderAsync(object model, CultureInfo culture, Func<ActionContext, IView> findView, Action<ViewContext>? beforeRender = null);
    }
}