# Email

- [Email](#email)
  - [Setup](#setup)
  - [Create and send an email](#create-and-send-an-email)
  - [View locations](#view-locations)
  - [Attachments](#attachments)
  - [Headers](#headers)

## Setup

The following sample binds settings from the configuration located in the section "Email" using SimpleInjector.

The settings contain sender Email and name, SMTP settings, Debug settings and CSS settings. Check the `EmailSettings` class for details.
```cs
container.RegisterEmail(options => Configuration.GetSection("Email").Bind(options));
```

## Create and send an email

To add new emails, following steps need to be done:

1) Add a new View (`.cshtml`) in the directory `Views/Emails/`.
2) Add a new ViewModel in the same namespace as the business logic that needs to send the mail.
3) Add the `EmailView` attribute to the ViewModel. The constructor argument is path to the view, relative to the `Views` directory without the `.cshtml` extension. Example: `[EmailView("Emails/Registration")]` points to `Views/Emails/Registration.cshtml`.
4) Extend the `EmailController` with a new method to render the view file and return the contents.
5) To send the mail in the business logic use the MediatR-command `SendEmail` and supply it with the view model. SendEmail renders the mail based on the view model and the `EmailViewAttribute`.

To check the visuals of the view file, use the [Swagger API](./api.md) to access the methods of the `EmailController`.

## View locations

The views that are used for the emails are configured on the `EmailViewAttribute`. To find the view, the Razor view engine looks in the folder `Views` by default. A view path of `Emails/FancyEmail` matches to `Views/Emails/FancyEmail.cshtml`.

If you want to place the Views in other folders, for example `/Emails`, you can simply configure this in the Razor options as follows:

```cs
services.Configure<RazorViewEngineOptions>(options => options.ViewLocationFormats.Add("/Emails/{0}" + RazorViewEngine.ViewExtension));
```

## Attachments

By default, only attachments in file://-Uris are supported. To allow adding attachments from other sources (eg. AWS S3), implement `IEmailAttachmentResolver` and register it SimpleInjector with
```cs
container.Collections.Append<IEmailAttachmentResolver, YourResolver>()
```

## Headers

No special Headers are sent per default, however with the `Headers` parameter on `SendEmail`, `MimeKit.Header`s can be added to the email (overrides all Headers defined in `EmailOptions.DefaultHeaders`). With `EmailOptions.DefaultHeaders` default headers can be set for all emails. Predefined sets of headers can be found in `EmailHeaders`:
- `DiscourageAutoReplies` includes [`Precedence:list`](https://www.rfc-editor.org/rfc/rfc3834#section-3.1.8), [`AutoSubmitted:generated`](https://www.rfc-editor.org/rfc/rfc3834#section-3.1.7), and [`X-Auto-Response-Suppress:All`](https://learn.microsoft.com/en-us/openspecs/exchange_server_protocols/ms-oxcmail/e489ffaf-19ed-4285-96d9-c31c42cab17f). They discourage email servers to sent auto replies.
