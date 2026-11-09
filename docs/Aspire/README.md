# Aspire Hosting

Extensions for the [Aspire](https://aspire.dev) AppHost. Tested with Aspire 13.6.

- [Aspire Hosting](#aspire-hosting)
  - [Compose stack](#compose-stack)
  - [Protected secrets](#protected-secrets)
    - [Where the secrets are kept](#where-the-secrets-are-kept)
    - [Limitations](#limitations)

The extension methods are in the namespace `Aspire.Hosting`, like Aspire's own, so they are available in the AppHost without a `using`.

## Compose stack

Aspire's containers show up in Docker Desktop as a flat list, mixed with everything else running on the machine. `UseComposeStack` labels them like Docker Compose does, so they are grouped as one stack:

```cs
var builder = DistributedApplication.CreateBuilder(args);
builder.UseComposeStack();      // stack "myapp" for MyApp.AppHost

var postgres = builder.AddPostgres("db")
   .WithPgAdmin()               // child resources are part of the stack, too
   .WithComposeService("postgres");   // optional: service name other than the resource name

builder.AddContainer("mailpit", "axllent/mailpit")
   .WithoutComposeStack();      // optional: leave a container out
```

- The stack name defaults to the AppHost's name without `.AppHost`; pass one to override it: `UseComposeStack("my-stack")`.
- Names are normalized to what Compose allows: lowercase letters, digits, `-` and `_`.
- Only in run mode; `aspire publish` and `aspire deploy` are not affected.
- Changing the labels changes the configuration of a persistent container (`ContainerLifetime.Persistent`), so Aspire recreates it once. Its volumes stay.

## Protected secrets

When a parameter is entered in the Aspire dashboard and saved, Aspire writes it to the AppHost's user secrets: a plaintext `secrets.json` in the user profile. `UseProtectedSecrets` keeps these values in the operating system's secret store for the current user instead:

```cs
var builder = DistributedApplication.CreateBuilder(args);
builder.UseProtectedSecrets();

var apiKey = builder.AddParameter("api-key", secret: true);   // entered in the dashboard as usual
```

- Stored secrets are loaded over the user secrets, so a stale value in `secrets.json` doesn't win. Environment variables and command line arguments still do.
- Aspire's deployment state (`aspire publish`/`aspire deploy`) stays in the user secrets.
- Where no store is available (Linux without a keyring, e.g. headless or WSL), the AppHost logs a warning at startup and uses the plaintext user secrets as before.

### Where the secrets are kept

| OS | Store |
|---|---|
| Windows | File encrypted with DPAPI for the current user, in `%LOCALAPPDATA%\Fusonic.Extensions.Aspire` |
| macOS | Keychain, service `Fusonic.Extensions.Aspire` |
| Linux | Secret Service via libsecret (e.g. GNOME Keyring), schema `Fusonic.Extensions.Aspire.secrets` |

The AppHost's secrets are identified by its `UserSecretsId` (or its name, if it has none). Both can be changed:

```cs
builder.UseProtectedSecrets(options =>
{
    options.Name = "my-app";
    options.Directory = "/some/where";   // the encrypted file on Windows
});
```

Uses the cache storage of [Microsoft.Identity.Client.Extensions.Msal](https://www.nuget.org/packages/Microsoft.Identity.Client.Extensions.Msal), the maintained cross-platform wrapper around these stores.

### Limitations

- **Generated parameter defaults** (e.g. a password Aspire generates for `AddPostgres` without one) are still saved to the plaintext user secrets: Aspire saves them through `IDistributedApplicationBuilder.UserSecretsManager`, which can't be replaced. Set such parameters explicitly (`AddParameter(..., secret: true)`) to keep them in the store.
- Values already in `secrets.json` stay there. Remove them by hand (`dotnet user-secrets remove <key>`) after entering them again in the dashboard.
- Builds on Aspire's experimental `IUserSecretsManager` (`ASPIREUSERSECRETS001`). A newer Aspire version may need a new version of this package.
