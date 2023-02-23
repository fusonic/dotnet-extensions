# Fusonic Extensions 
[![build](https://github.com/fusonic/dotnet-extensions/workflows/build/badge.svg)](https://github.com/fusonic/dotnet-extensions/actions?query=workflow%3Abuild)
[![nuget](https://img.shields.io/badge/fusonic%20extensions-7.0.4-blue)](https://www.nuget.org/packages?q=Fusonic.Extensions)
[![licence](https://img.shields.io/github/license/fusonic/dotnet-extensions)](https://github.com/fusonic/dotnet-extensions/blob/main/LICENSE)

The **Fusonic Extensions** project aggregates several thoughtfully designed, reusable class libraries, which can be used in modern .NET application development. Fusonic successfully uses these libraries in its own individual projects. Some packages are general purpose libs which can be used by any kind of .net project (console, web, rich client). Other packages may be best suited for ASP<span>.</span>NET Core development. Because we believe that they can be useful for the broader community, we open source it under a permissive license.

Currently primary development takes place at a private repository at Gitlab.com.
The project on Github.com is updated on a daily basis, but does not include any issues managed at Gitlab. However, we are happily accepting issues and pull requests on Github as well! Feel free to open an issue or merge request.
If we see broader community engagement in the future, we may consider switching our primary development to Github.

Latest Fusonic Extensions supports the platforms:

* .NET 7.0

Packages
===============

Fusonic Extensions consists of several thoughtfully designed class libraries which are split into logical areas. Documentation is provided per package. In order to increase versioning and deployment complexity, all packages share a common semantic versioning scheme.

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.AspNetCore.svg?label=Fusonic.Extensions.AspNetCore&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.AspNetCore/)
Provides ASP<span>.</span>NET Core extensions (Middelwares, Utilities ...). See the [documentation](docs/AspNetCore/README.md) for more details.

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.Common.svg?label=Fusonic.Extensions.Common&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.Common/)
Adds serval small helpers and common abstractions. See the [documentation](docs/Common/README.md) for more details.

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.Email.svg?label=Fusonic.Extensions.Email&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.Email/)
Adds support for sending emails. See the [documentation](docs/Email/README.md) for more details.

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.EntityFrameworkCore.svg?label=Fusonic.Extensions.EntityFrameworkCore&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.EntityFrameworkCore/)
Extensions for EF Core.

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.Hangfire.svg?label=Fusonic.Extensions.Hangfire&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.Hangfire/)
Provides Hangfire extensions, especially suited for CQRS developement. (Out of band processing). See the [documentation](docs/Hangfire/README.md) for more details.

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.Hosting.svg?label=Fusonic.Extensions.Hosting&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.Hosting/)
Provides services and extensions for hosting. See the [documentation](docs/Hosting/README.md) for more details.

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.MediatR.svg?label=Fusonic.Extensions.MediatR&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.MediatR/)
Provides abstractions for MediatR. See the [documentation](docs/MediatR/README.md) for more details.

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.UnitTests.svg?label=Fusonic.Extensions.UnitTests&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.UnitTests/)
Xunit-based testing base classes with support for dependency injection. Libraries supporting specific DI containers (SimpleInjector, ServiceProvider) are in separate packages. See the [unit test documentation](docs/UnitTests/README.md) for more details.

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.UnitTests.ServiceProvider.svg?label=Fusonic.Extensions.UnitTests.ServiceProvider&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.UnitTests.ServiceProvider/)
Xunit-based testing base classes. Supports dependency injection with Microsofts Dependency Injection framework (ServiceProvider).. See the [unit test documentation](docs/UnitTests/README.md) for more details.

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.UnitTests.SimpleInjector.svg?label=Fusonic.Extensions.UnitTests.SimpleInjector&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.UnitTests.SimpleInjector/)
Xunit-based testing base classes. Supports dependency injection with SimpleInjector.. See the [unit test documentation](docs/UnitTests/README.md) for more details.

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.UnitTests.EntityFrameworkCore.svg?label=Fusonic.Extensions.UnitTests.EntityFrameworkCore&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.UnitTests.EntityFrameworkCore/)
Adds database support using EF Core to the unit tests. See the [unit test documentation](docs/UnitTests/README.md) for more details.

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql.svg?label=Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql/)
Adds support for database tests using EF Core with PostgreSQL. See the [unit test documentation](docs/UnitTests/README.md) for more details.

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer.svg?label=Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.UnitTests.EntityFrameworkCore.SqlServer/)
Adds support for database tests using EF Core with Microsoft SQL Server. See the [unit test documentation](docs/UnitTests/README.md) for more details.


Important information
===============

Goals
--------------------

- Reusable class libraries with high quality and high reusability
- Supports quality and velocity in fusonic projects
- More quality because of strong testing focus
- Stable


Non-Goals
--------------------

- We do not implement **framework-code** but focus on **re-usable class libraries**
- No DI abstractions necessary (SimpleInjector or MediatR are committed core libraries used in Fusonic Extensions)


Naming
--------------------

Fusonic.Extensions.* (Ex. Fusonic.Extensions.Abstractions)

"AspNetCore" specific extensions land in "AspNetCore" "namespace".


What code can be added to Fusonic Extensions?
--------------------

- Reusable abstract logic which is generally valid and meaningful.
- Stability: Code should not be very volatile and, at best, tested. It should change as little as possible to the public API.
- Extensible: Logic should, if reasonably practicable, be extensible and flexible (eg virtual methods)


Standards (QA/Versioning, Unit-Tests)
--------------------

- All code must be unit-tested
- Breaking changes shall be avoided. We use semantic versioning: https://semver.org/
- Major releases are aligned with the yearly [.NET release schedule](https://github.com/dotnet/core/blob/main/roadmap.md).


Allowed base dependencies
--------------------
If possible, only .NET BCL libraries should be referenced. Especially with abstract code. We try to reference as few dependencies as possible.
However, we agree on the following core technologies, if absolutely necessary for the meaningfulness of the package:

* SimpleInjector
* MediatR
* Entity FrameworkCore


Integration Workflow
--------------------
1. Create a proposal on Gitlab.com (fusonic developers) or Github.com (external developers)
2. Detailed description of the use cases and API (also with MR request)
3. Notify Fusonic Extensions "Design member board" (David Roth, Johannes Hartmann)
4. Discussion in GL Issue or Mr Request or Design Meeting
5. New APIs require the approval of at least one board member
6. New APIs should be communicated (announcement)
