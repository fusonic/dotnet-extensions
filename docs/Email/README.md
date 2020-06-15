# Email

- [Email](#email)
  - [Setup](#setup)
  - [Create and send an email](#create-and-send-an-email)
  - [View locations](#view-locations)

## Setup

The following sample binds settings from the configuration located in the section "Email" using SimpleInjector.

The settings contain sender Email and name, SMTP settings, Debug settings and CSS settings. Check the `EmailSettings` class for details.
```cs
Container.RegisterEmail(options => Configuration.GetSection("Email").Bind(options));
```

## Create and send an email

To add new emails, following steps need to be done:

1) Add a new View (`.cshtml`) in the directory `Views/Emails/`.
2) Add a new ViewModel in the same namespace as the business logic that needs to send the mail.
3) Add the `EmailView` attribute to the ViewModel. The constructor argument is path to the view, relative to the `Views` directory without the `.cshtml` extension. Example: `[EmailView("Emails/Registration")]` points to `Views/Emails/Registration.cshtml`.
4) Extend the `EmailController` with a new method to render the view file and return the contents.
5) To send the mail in the business logic use `SendEmail.Command` and supply it with the view model. SendEmail renders the mail based on the view model and the `EmailViewAttribute`.

To check the visuals of the view file, use the [Swagger API](./api.md) to access the methods of the `EmailController`.

## View locations

The views that are used for the emails are configured on the `EmailViewAttribute`. To find the view, the Razor view engine looks in the folder `Views` by default. A view path of `Emails/FancyEmail` matches to `Views/Emails/FancyEmail.cshtml`.

If you want to place the Views in other folders, for example `/Emails`, you can simply configure this in the Razor options as follows:

```cs
services.Configure<RazorViewEngineOptions>(options => options.ViewLocationFormats.Add("/Emails/{0}" + RazorViewEngine.ViewExtension));
```