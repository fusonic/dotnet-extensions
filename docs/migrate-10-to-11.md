# Migrating from Fusonic Extensions 10 to 11

Fusonic Extensions 11 targets .NET 11 only. This guide lists what consumers have to change when they update from 10.x.

## Table of contents

- [.NET 11](#net-11)
- [Dependencies](#dependencies)
- [Email](#email)
- [UnitTests](#unittests)
- [PostgreSQL and SQL Server](#postgresql-and-sql-server)

## .NET 11

- All packages target `net11.0` only. Update the `TargetFramework` of your projects and the SDK in `global.json`, CI and Docker images (`mcr.microsoft.com/dotnet/sdk:11.0`, `mcr.microsoft.com/dotnet/aspnet:11.0`).
- The packages are compiled with [runtime async](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/runtime#runtime-async). Your projects don't have to enable it. To enable it in your projects too, add `runtime-async=on` to `<Features>`.
- For the breaking changes of .NET 11 itself see [Breaking changes in .NET 11](https://learn.microsoft.com/en-us/dotnet/core/compatibility/11).

## Dependencies

The packages now reference these major versions. Update your own references to them to avoid version conflicts:

| Package | 10.x | 11.x |
|-|-|-|
| `Microsoft.EntityFrameworkCore.*`, `Microsoft.Extensions.*` | 10.0 | 11.0 |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.0 | 11.0 |
| `MailKit` (and `MimeKit.Core` instead of `MimeKit`) | 4.x | 5.0 |
| `Aspire.Hosting` | 13.x | 17.0 |

Check the release notes of [EF Core 11](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/breaking-changes), [Npgsql 11](https://www.npgsql.org/efcore/release-notes/11.0.html) and MailKit 5 for changes in your own code. The most likely ones:

- EF Core 11: `Migrate()`/`MigrateAsync()` throws if the `DbContext` has no migrations.
- EF Core 11: `UseSqlServer` targets compatibility level 160 (SQL Server 2022) by default. Use `UseCompatibilityLevel(...)` for older servers.
- Npgsql 11: `Multiplexing` was removed from the connection string, and EF Core targets PostgreSQL 16 by default (`SetPostgresVersion(...)` for older servers).
- MailKit 5: `MimeKit` types moved to the `MimeKit.Core` assembly. Packages that still depend on `MimeKit` 4 (e.g. netDumbster) can't be used next to it.

## Email

- `SendEmailBccDecorator` was removed. The `SendEmail` handler applies `EmailOptions.BccAddress` itself. Remove any own registration of the decorator; `RegisterEmail`/`AddEmail` no longer register a decorator.
- `AddEmail` no longer needs Scrutor.

## UnitTests

- `DependencyInjectionUnitTest.ScopedAsync` now disposes the scope it creates. Don't use services resolved inside `ScopedAsync` after it returns.
- `PostgreSqlUtil.CreateTestDbTemplate` and `SqlServerTestUtil.CreateTestDbTemplate` apply the migrations by default (`useMigrations: true`), which throws with EF Core 11 if the `DbContext` has no migrations. Pass `useMigrations: false` for such contexts.

## PostgreSQL and SQL Server

Not required, but the samples in the [UnitTests docs](https://github.com/fusonic/dotnet-extensions/blob/main/docs/UnitTests/README.md) now use `postgres:18-alpine` and SQL Server 2025. PostgreSQL 18 images keep their data in `/var/lib/postgresql` instead of `/var/lib/postgresql/data`, so move the tmpfs mount when you update the image:

```csharp
var container = new PostgreSqlBuilder("postgres:18-alpine")
    .WithTmpfsMount("/var/lib/postgresql")
    .WithTmpfsMount("/dev/shm")
    .Build();
```
