<div align="center">

<img src="icon.png" alt="Fusonic Extensions" width="120" />

# Fusonic Extensions

**Thoughtfully designed, reusable .NET libraries. Battle-tested in real Fusonic projects.**

[![build](https://img.shields.io/github/actions/workflow/status/fusonic/dotnet-extensions/pipeline.yml?branch=main&style=for-the-badge&logo=github&label=build)](https://github.com/fusonic/dotnet-extensions/actions?query=workflow%3Abuild)
[![nuget](https://img.shields.io/nuget/v/Fusonic.Extensions.Common.svg?style=for-the-badge&logo=nuget&label=nuget&color=004880)](https://www.nuget.org/packages?q=Fusonic.Extensions)
[![.NET 11](https://img.shields.io/badge/.NET-11.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![license](https://img.shields.io/github/license/fusonic/dotnet-extensions?style=for-the-badge&color=blue)](LICENSE)

[Packages](#-packages) · [Getting started](#-getting-started) · [Principles](#-principles) · [Contributing](#-contributing) · [Migration](#-migration)

</div>

---

## ✨ Why Fusonic Extensions?

Fusonic Extensions aggregates small, focused class libraries for modern .NET application development. We use them in our own projects every day, and because we believe they are useful for the broader community, they are open source under a permissive license.

|   |   |
|---|---|
| 🧩 **Pick what you need** | Every package stands on its own. Use one, use all. |
| 💉 **Two containers, zero abstractions** | Packages register directly with **SimpleInjector** and **Microsoft.Extensions.DependencyInjection**. |
| 🧪 **Tested** | All code is unit-tested, including against real databases and mail servers via Testcontainers. |
| 🔒 **Stable** | Semantic versioning, as few breaking changes as possible, yearly release cycle aligned with .NET. |
| 🪶 **Lean** | We reference as few dependencies as we can get away with. |

> [!NOTE]
> Primary development takes place in a private repository on GitLab.com. This GitHub project is updated daily, but does not include the issues managed on GitLab. Issues and pull requests are very welcome here as well!

## 🚀 Getting started

All packages are available on [NuGet](https://www.nuget.org/packages?q=Fusonic.Extensions):

```bash
dotnet add package Fusonic.Extensions.Common
```

All packages share one common version, so mixing and matching is easy. The `main` branch targets **.NET 11.0**.

## 📦 Packages

Documentation is provided per package.

### Application

| Package | What it does | Docs |
|---|---|---|
| [![Fusonic.Extensions.Common](https://img.shields.io/nuget/v/Fusonic.Extensions.Common.svg?label=Common&style=flat-square&logo=nuget)](https://www.nuget.org/packages/Fusonic.Extensions.Common/) | Small helpers and common abstractions. | [📖](docs/Common/README.md) |
| [![Fusonic.Extensions.Mediator](https://img.shields.io/nuget/v/Fusonic.Extensions.Mediator.svg?label=Mediator&style=flat-square&logo=nuget)](https://www.nuget.org/packages/Fusonic.Extensions.Mediator/) | A simple mediator implementation. | [📖](docs/Mediator/README.md) |
| [![Fusonic.Extensions.Email](https://img.shields.io/nuget/v/Fusonic.Extensions.Email.svg?label=Email&style=flat-square&logo=nuget)](https://www.nuget.org/packages/Fusonic.Extensions.Email/) | Support for sending emails. | [📖](docs/Email/README.md) |
| [![Fusonic.Extensions.Hangfire](https://img.shields.io/nuget/v/Fusonic.Extensions.Hangfire.svg?label=Hangfire&style=flat-square&logo=nuget)](https://www.nuget.org/packages/Fusonic.Extensions.Hangfire/) | Hangfire extensions, especially suited for CQRS development (out-of-band processing). | [📖](docs/Hangfire/README.md) |

### Hosting & infrastructure

| Package | What it does | Docs |
|---|---|---|
| [![Fusonic.Extensions.AspNetCore](https://img.shields.io/nuget/v/Fusonic.Extensions.AspNetCore.svg?label=AspNetCore&style=flat-square&logo=nuget)](https://www.nuget.org/packages/Fusonic.Extensions.AspNetCore/) | ASP.NET Core extensions: middlewares, utilities and more. | [📖](docs/AspNetCore/README.md) |
| [![Fusonic.Extensions.Hosting](https://img.shields.io/nuget/v/Fusonic.Extensions.Hosting.svg?label=Hosting&style=flat-square&logo=nuget)](https://www.nuget.org/packages/Fusonic.Extensions.Hosting/) | Services and extensions for hosting. | [📖](docs/Hosting/README.md) |
| [![Fusonic.Extensions.Aspire.Hosting](https://img.shields.io/nuget/v/Fusonic.Extensions.Aspire.Hosting.svg?label=Aspire.Hosting&style=flat-square&logo=nuget)](https://www.nuget.org/packages/Fusonic.Extensions.Aspire.Hosting/) | Aspire AppHost extensions: group containers as a Docker Compose stack and keep secrets entered in the dashboard in the OS's secret store. | [📖](docs/Aspire/README.md) |
| [![Fusonic.Extensions.EntityFrameworkCore](https://img.shields.io/nuget/v/Fusonic.Extensions.EntityFrameworkCore.svg?label=EntityFrameworkCore&style=flat-square&logo=nuget)](https://www.nuget.org/packages/Fusonic.Extensions.EntityFrameworkCore/) | Extensions for EF Core. | [📖](docs/EntityFrameworkCore/README.md) |

### Testing

xUnit v3 based base classes with dependency injection support for SimpleInjector and Microsoft DI. All four packages share the [unit test documentation](docs/UnitTests/README.md).

| Package | What it does |
|---|---|
| [![Fusonic.Extensions.UnitTests](https://img.shields.io/nuget/v/Fusonic.Extensions.UnitTests.svg?label=UnitTests&style=flat-square&logo=nuget)](https://www.nuget.org/packages/Fusonic.Extensions.UnitTests/) | Test base classes with dependency injection. |
| [![Fusonic.Extensions.UnitTests.EntityFrameworkCore](https://img.shields.io/nuget/v/Fusonic.Extensions.UnitTests.EntityFrameworkCore.svg?label=UnitTests.EntityFrameworkCore&style=flat-square&logo=nuget)](https://www.nuget.org/packages/Fusonic.Extensions.UnitTests.EntityFrameworkCore/) | Database support for unit tests using EF Core. |
| [![Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql](https://img.shields.io/nuget/v/Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql.svg?label=UnitTests.EntityFrameworkCore.Npgsql&style=flat-square&logo=nuget)](https://www.nuget.org/packages/Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql/) | Database tests using EF Core with PostgreSQL. |
| [![Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer](https://img.shields.io/nuget/v/Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer.svg?label=UnitTests.EntityFrameworkCore.SqlServer&style=flat-square&logo=nuget)](https://www.nuget.org/packages/Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer/) | Database tests using EF Core with Microsoft SQL Server. |

## 🧭 Principles

### 🎯 Goals

- Reusable class libraries with high quality and high reusability
- Support quality and velocity in Fusonic projects
- More quality through a strong testing focus
- Stability

### 🚫 Non-goals

- We do not implement **framework code**. We focus on **reusable class libraries**.
- No DI abstractions: packages register their services directly with SimpleInjector and Microsoft.Extensions.DependencyInjection.

### 🏷️ Naming

Packages are named `Fusonic.Extensions.*`. ASP.NET Core specific extensions live in the `AspNetCore` namespace.

### ✅ What code can be added?

- **Reusable:** abstract logic that is generally valid and meaningful.
- **Stable:** code should not be volatile and, at best, tested. The public API should change as little as possible.
- **Extensible:** where reasonably practicable, logic should be flexible (e.g. `virtual` methods).

### 🛡️ Standards

- All code must be unit-tested.
- Breaking changes are avoided. We use [semantic versioning](https://semver.org/).
- Major releases are aligned with the yearly [.NET release schedule](https://github.com/dotnet/core/blob/main/roadmap.md).

### 🔗 Allowed base dependencies

If possible, only .NET BCL libraries are referenced, especially for abstract code. We try to reference as few dependencies as possible. If absolutely necessary for the meaningfulness of a package, we agree on these core technologies:

- **SimpleInjector**
- **Scrutor** (for Microsoft.Extensions.DependencyInjection)
- **Entity Framework Core**

## 🤝 Contributing

New APIs go through a lightweight design process:

1. Create a proposal on GitLab.com (Fusonic developers) or GitHub.com (external developers).
2. Describe the use cases and the API in detail (also in the merge request).
3. Notify the Fusonic Extensions design board (David Roth, Johannes Hartmann).
4. Discuss in the GitLab issue, the merge request or a design meeting.
5. New APIs require the approval of at least one board member.
6. New APIs should be communicated (announcement).

## 🔄 Migration

Breaking changes between major versions and the changes they require in your code are described in the migration guides:

- [Migrating from 10 to 11](docs/migrate-10-to-11.md)

---

<div align="center">

Made with ❤️ by [Fusonic](https://www.fusonic.net) · Licensed under the [MIT License](LICENSE)

</div>
