#!/bin/bash

dotnet build src/Example.sln
dotnet tool install -g Fusonic.Extensions.UnitTests.Tools.PostgreSql
pgtestutil template -c "Host=127.0.0.1;Port=5433;Database=example_test;Username=postgres;Password=postgres" -a "src/Example.Database.Tests/bin/Debug/net6.0/Example.Database.Tests.dll"