# Fusonic Extensions

The goal of **Fusonic Extensions** is to provide .NET application developers with reusable librariers that promotes best practice to steer developers towards the pit of success.
Only well tested and proven code lands in Fusonic Extensions.

The following platforms are supported:

* .NET Standard 2.0
* .NET Core 2.2

Packages
===============

Fusonic Extensions consists of several thoughtfully designed class librariers which are split into logical areas, to allow great reusability in projects.
Some packages are general purpose libs which can be used by any kind of .net project (console, web, rich client). Other packages may be best suited for Asp.net core development.

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.Abstractions.svg?label=Fusonic.Extensions.Abstractions&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.Abstractions/)
Provides high level abstractions which should not be tied to a specific implementation. (E.x. Attributes, Interfaces).

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.Hangfire.svg?label=Fusonic.Extensions.Hangfire&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.Hangfire/)
Provides Hangfire extensions, especially suited for CQRS developement. (Out of band processing).

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.AspnetCore.Http.svg?label=Fusonic.Extensions.AspnetCore.Http&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.AspnetCore.Http/)
Provides aspnet core http extensions (Middelwares, Utilities ...)

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.Validation.svg?label=Fusonic.Extensions.Validation&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.Validation/)
Provides validation pipeline and recursive DataAnnotations annotation

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.Validation.Mvc.svg?label=Fusonic.Extensions.Validation.Mvc&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.Validation.Mvc/)
Provides an integration for validation to aspnetcore mvc

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.UnitTests.svg?label=Fusonic.Extensions.UnitTests&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.UnitTests/)
Xunit-based testing base classes. Supports DI with SimpleInjector, MediatR-event-recordings, Lifetime-Scoped calls, a test context and so on. See the [unit test documentation](src/UnitTests/docs/README.md) for more details.

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore.svg?label=Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore/)
Adds basic database support using EF Core to the unit tests. Does not ship any database providers. Additional adapters are required. See the [unit test documentation](src/UnitTests/docs/README.md) for more details.

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.UnitTests.Adapters.InMemoryDatabase.svg?label=Fusonic.Extensions.UnitTests.Adapters.InMemoryDatabase&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.UnitTests.Adapters.InMemoryDatabase/)
Adds support for database tests using the InMemoryDatabase for EF Core. See the [unit test documentation](src/UnitTests/docs/README.md) for more details.

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.UnitTests.Adapters.PostgreSql.svg?label=Fusonic.Extensions.UnitTests.Adapters.PostgreSql&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.UnitTests.Adapters.PostgreSql/)
Adds support for database tests using PostgreSql for EF Core. See the [unit test documentation](src/UnitTests/docs/README.md) for more details.

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
- No DI abstractions necessary (SimpleInjector or Mediatr are committed core librarires used in Fusonic Extensions)
- Open-Source: At the moment it is not planned to run the project as open-source. We will focus on the use cases and needs of Fusonic customer projects and will develop on Gitlab.



Naming
--------------------

Fusonic.Extensions.* (Ex. Fusonic.Extensions.Abstractions)

"AspnetCore" specific extensions land in "AspnetCore" "namespace".


What code can be added to Fusonic Extensions?
--------------------

- Generic logic  which are generally valid and meaningful.
- Stability: Logic should not be very volatile and, at best, tested. It should change as little as possible to the public API.
- Extensible: Logic should, if reasonably practicable, be extensible and flexible (eg virtual methods)


Standards (QA/Versioning, Unit-Tests)
--------------------

- All code  must be unit-tested
- We do not introduce any breaking changes. We use semantic versioning: https://semver.org/


Allowed base dependencies
--------------------
If possible, only .NET standard references should be added. Especially with abstract code. We try to reference as few dependencies as possible.
However, we agree on the following core technologies, if absolutely necessary for the meaningfulness of the package:

* SimpleInjector
* MediatR
* Entity FrameworkCore


Integration Workflow
--------------------
1. Create a proposal on gitlab
2. Detailed description of the use cases and API (also with MR request)
3. Notify Fusonic Extensions "Design member board" (Droth, JHartmann, LBickel)
4. Discussion in GL Issue or Mr Request or Design Meeting
5. New APIs require the approval of at least one board member
6. New APIs should be communicated (announcement)