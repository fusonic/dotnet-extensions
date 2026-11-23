# Email

Package: `Fusonic.Extensions.Email`

Renders emails from Razor views (`.cshtml`) or Razor components (`.razor`) and sends them via SMTP (MailKit). Emails are sent with the Mediator command `SendEmail`.

- [Email](#email)
  - [Setup](#setup)
    - [Prerequisites](#prerequisites)
    - [Microsoft.Extensions.DependencyInjection](#microsoftextensionsdependencyinjection)
    - [SimpleInjector](#simpleinjector)
    - [Options](#options)
  - [Send an email](#send-an-email)
    - [Sending in the background](#sending-in-the-background)
  - [Razor Views (MVC)](#razor-views-mvc)
    - [View locations](#view-locations)
    - [Set a Subject](#set-a-subject)
  - [Razor Components (Blazor)](#razor-components-blazor)
    - [Set a Subject](#set-a-subject-1)
  - [Attachments](#attachments)
  - [Headers](#headers)
  - [Post Processor](#post-processor)
    - [Mjml Sample](#mjml-sample)

## Setup

All types are in the namespace `Fusonic.Extensions.Email`.

### Prerequisites

The package renders emails with the ASP.NET Core Razor engine. Register the following framework services in your `IServiceCollection`:

```cs
services.AddLogging();
services.AddLocalization(options => options.ResourcesPath = "Resources");
services.AddRazorPages().AddViewLocalization(); // Razor views: IRazorViewEngine, ITempDataProvider, IViewLocalizer
services.AddRazorComponents();                  // Razor components
services.AddScoped<HtmlRenderer>();             // Microsoft.AspNetCore.Components.Web
```

`AddMvc()` or `AddControllersWithViews()` work as well, as long as view localization is enabled.

The package also needs an `IMediator` from [Fusonic.Extensions.Mediator](https://github.com/fusonic/dotnet-extensions/blob/main/docs/Mediator/README.md) to send `SendEmail`.

### Microsoft.Extensions.DependencyInjection

```cs
using Fusonic.Extensions.Email;
using Fusonic.Extensions.Mediator;
using Fusonic.Extensions.Mediator.DependencyInjection;

services.AddScoped<IMediator>(sp => new ServiceProviderMediator(sp));
services.AddEmail(options => configuration.GetSection("Email").Bind(options));
```

`AddEmail` registers the `SendEmail` handler. If you use the out-of-band decorators of [Fusonic.Extensions.Hangfire](https://github.com/fusonic/dotnet-extensions/blob/main/docs/Hangfire/README.md), call `AddEmail` before `AddOutOfBandDecorators`.

### SimpleInjector

`RegisterEmail` resolves `IViewLocalizer` and `IStringLocalizerFactory` from the container, so the framework services must be cross-wired:

```cs
using Fusonic.Extensions.Email;
using Fusonic.Extensions.Mediator;

container.RegisterEmail(options => configuration.GetSection("Email").Bind(options));
container.RegisterMediator(services, [typeof(Program).Assembly]);

services.AddSimpleInjector(container, options => options.AutoCrossWireFrameworkComponents = true);
// After building the service provider:
app.Services.UseSimpleInjector(container);
```

`RegisterEmail` registers the `SendEmail` handler itself; you don't need to pass the email assembly to `RegisterMediator`. With the out-of-band decorators of Hangfire, call `RegisterEmail` after `RegisterOutOfBandDecorators`.

### Options

`EmailOptions` are validated on registration. A `ValidationException` is thrown if they are invalid.

| Option | Description |
|---|---|
| `SenderAddress` | **Required.** Sender email address. |
| `SenderName` | Sender display name. |
| `SmtpServer` | **Required**, unless `StoreInDirectory` is set. |
| `SmtpPort` | Default `465`. |
| `EnableSsl` | Default `true`. |
| `SmtpUsername`, `SmtpPassword` | SMTP credentials. |
| `StoreInDirectory` | Writes emails to this directory instead of sending them (development only). SMTP settings are ignored. |
| `SubjectPrefix` | Prefixes every subject, e.g. `"[MyBranch] "` for review environments. |
| `BccAddress` | BCC address for every email. A `BccRecipient` set on `SendEmail` has precedence. |
| `CssPath` | CSS file used for inlining. Default `[assemblyDir]/wwwroot/assets/emails/email.css`. `null` disables CSS. |
| `DefaultHeaders` | Headers added to every email, see [Headers](#headers). |
| `IgnoredDomains` | Recipients with these domains are not sent an email. The domain `invalid` is always ignored. |
| `RazorContentPostProcessor`, `BlazorContentPostProcessor` | See [Post Processor](#post-processor). |

Example `appsettings.json`:
```json
{
    "Email": {
        "SenderAddress": "noreply@example.com",
        "SenderName": "Example",
        "SmtpServer": "smtp.example.com",
        "SmtpUsername": "user",
        "SmtpPassword": "secret"
    }
}
```

## Send an email

Send the `SendEmail` command with a view model (Razor view) or component model (Razor component):

```cs
await mediator.Send(new SendEmail(
    Recipient: "jane@example.com",
    RecipientDisplayName: "Jane Doe",
    Culture: new CultureInfo("de-AT"),
    ViewModel: new RegistrationEmailViewModel { Name = "Jane" }));
```

Optional parameters:
- `SubjectKey`: overrides the subject key of the view/component.
- `SubjectFormatParameters`: format parameters for the translated subject (`string.Format(subject, parameters)`).
- `BccRecipient`, `ReplyTo`: additional addresses.
- `Attachments`: see [Attachments](#attachments).
- `Headers`: see [Headers](#headers).

The email is rendered in the given `Culture`.

### Sending in the background

The handler of `SendEmail` is marked with `[OutOfBand]`. With the out-of-band decorators of [Fusonic.Extensions.Hangfire](https://github.com/fusonic/dotnet-extensions/blob/main/docs/Hangfire/README.md) registered, the email is sent in a Hangfire background job. Without them, it is sent synchronously.

## Razor Views (MVC)

1) Add a view (`.cshtml`), e.g. `Views/Emails/Registration.cshtml`.
2) Add a view model and mark it with `[EmailView]`. The argument is the path to the view, relative to the `Views` directory, without the `.cshtml` extension:
   ```cs
   [EmailView("Emails/Registration")]
   public class RegistrationEmailViewModel
   {
       public required string Name { get; init; }
   }
   ```
3) Send `SendEmail` with an instance of the view model.

### View locations

The Razor view engine looks in the folder `Views` by default. A view path of `Emails/FancyEmail` matches `Views/Emails/FancyEmail.cshtml`.

To place the views in other folders, for example `/Emails`, configure the Razor options:

```cs
services.Configure<RazorViewEngineOptions>(options => options.ViewLocationFormats.Add("/Emails/{0}" + RazorViewEngine.ViewExtension));
```

### Set a Subject
By default the subject key is `Subject`. It is resolved with the view localizer from the resource file of the view. To use a different key, set it on the attribute: `[EmailView("Emails/Registration", SubjectKey = "RegistrationSubject")]`. If no translation is found, the key itself is used as subject.

## Razor Components (Blazor)

1) Add a component (`.razor`). It can be placed in any folder or assembly.
2) Add a component model implementing `IComponentModel<T>` (namespace `Fusonic.Extensions.AspNetCore.Blazor`), where `T` is the type of the component:
   ```cs
   public class RegistrationEmailModel : IComponentModel<RegistrationEmail>
   {
       public required string Name { get; init; }
   }
   ```
3) Send `SendEmail` with an instance of the component model as `ViewModel`.

### Set a Subject
By default the subject key is `Subject`. It is resolved from the resource file of the component. To use a different key, implement `IProvideEmailSubject` on the component model: `string IProvideEmailSubject.SubjectKey => "RegistrationSubject";`. If no translation is found, the key itself is used as subject.

## Attachments

Pass attachments as `Attachment(Name, Source)` to `SendEmail`. By default only `file://` URIs are supported. To load attachments from other sources (e.g. AWS S3), implement `IEmailAttachmentResolver` and register it:

```cs
// SimpleInjector
container.Collection.Append<IEmailAttachmentResolver, YourResolver>();

// Microsoft.Extensions.DependencyInjection
services.AddSingleton<IEmailAttachmentResolver, YourResolver>();
```

## Headers

No special headers are sent by default.
- `EmailOptions.DefaultHeaders` sets headers for all emails.
- The `Headers` parameter of `SendEmail` adds headers to a single email. Headers with the same name as a default header replace it; all other default headers are still sent.

Predefined sets of headers can be found in `EmailHeaders`:
- `DiscourageAutoReplies` includes [`Precedence: list`](https://www.rfc-editor.org/rfc/rfc3834#section-3.1.8), [`Auto-Submitted: generated`](https://www.rfc-editor.org/rfc/rfc3834#section-3.1.7) and [`X-Auto-Response-Suppress: All`](https://learn.microsoft.com/en-us/openspecs/exchange_server_protocols/ms-oxcmail/e489ffaf-19ed-4285-96d9-c31c42cab17f). They discourage email servers from sending auto replies.

## Post Processor
`EmailOptions.RazorContentPostProcessor` (`.cshtml`) and `EmailOptions.BlazorContentPostProcessor` (`.razor`) process the rendered HTML before it is sent. By default both inline the CSS into the tags (`CssInliner.Inline`). Set them to `null` to disable post processing.

The processors get a `RazorPostProcessContext` (`Html`, `ViewModel`, `EmailViewAttribute`) or a `BlazorPostProcessContext` (`Html`, `ComponentModel`) and return the final HTML.

### Mjml Sample
With the following options, all Blazor emails are processed by [Mjml.Net](https://github.com/SebastianStehle/mjml-net) before being sent:

```cs
var mjmlRenderer = new MjmlRenderer();

services.AddEmail(o => // or container.RegisterEmail
{
    // Other configurations...

    o.BlazorContentPostProcessor = async ctx =>
    {
        var result = await mjmlRenderer.RenderAsync(ctx.Html);
        return result.Html;
    };
});
```
