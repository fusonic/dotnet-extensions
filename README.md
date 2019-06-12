# Fusonic Extensions

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.Abstractions.svg?label=Fusonic.Extensions.Abstractions&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.Abstractions/)

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.Hangfire.svg?label=Fusonic.Extensions.Hangfire&style=plastic)](https://www.nuget.org/packages/Fusonic.Extensions.Hangfire/)

The goal of **Fusonic Extensions** is to provide .NET application developers with reusable librariers that promotes best practice to steer developers towards the pit of success.
Only well tested and proven code lands in Fusonic Extensions.

The following platforms are supported:

* .NET Standard 2.0

Packages
===============

Fusonic Extensions consists of several thoughtfully designed class librariers which are split into logical areas, to allow great reusability in projects.
Some packages are general purpose libs which can be used by any kind of .net project (console, web, rich client). Other packages may be best suited for Asp.net core development.

Abstractions
--------------------

Provides high level abstractions which should not be tied to a specific implementation. (E.x. Attributes, Interfaces).

Hangfire
--------------------

Provides Hangfire extensions, especially suited for CQRS developement. (Out of band processing).