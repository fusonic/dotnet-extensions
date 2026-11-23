# Instructions

Fusonic Extensions: reusable .NET class libraries, published as NuGet packages. Goals, non-goals and allowed base dependencies are in `README.md`.

## Principles

- **YAGNI**: Build only what the current task needs. No speculative abstractions, options, extension points, or "might need later" code.
- **KISS**: Prefer the simplest solution that fully solves the problem. Fewer layers, fewer moving parts, plain code over clever code.
- **Complete changes**: YAGNI never justifies dropping validation at trust boundaries, error handling that prevents data loss, or security. Finish every part the change needs, including the callers, tests, fixtures and docs it breaks.
- **Bug fixes**: Before changing a shared function's behaviour, find every caller, then fix the root cause once in the shared code.
- **No redundant comments**: Never write a comment that restates what the code already says. Comment only what the code cannot show: a non-obvious reason, constraint or workaround.
- **Public API**: Every public type is used by other projects. Avoid breaking changes (semantic versioning); when one is unavoidable, call it out in the merge request and describe the required changes for consumers in the migration guide of the upcoming major version (currently `docs/migrate-10-to-11.md`). Keep logic extensible where reasonable (e.g. `virtual` methods). New public APIs need the approval of a design board member (see `Integration Workflow` in `README.md`), so describe them in the merge request.
- **Dependencies**: Reference as few packages as possible; check the allowed base dependencies in `README.md` before adding one.

## Structure

- Each package lives in `src/<Package>/src`, its tests in `src/<Package>/test` and its documentation in `docs/<Package>/README.md`, which is packed into the NuGet package (see [Documentation](#documentation)). Exceptions: `Aspire.Hosting` is documented in `docs/Aspire`; the four `UnitTests*` packages share `docs/UnitTests`; `UnitTests` is tested through the other test projects. The csproj's `<None Include="...README.md" Pack="true">` shows which doc a package ships.
- A new package needs: a `src/<Package>/src/<Package>.csproj` with `AssemblyName`/`RootNamespace` `Fusonic.Extensions.<Package>`, `Description` and `PackageReadmeFile` (copy an existing csproj), a test project, entries in `Extensions.slnx` (`/Tests/` folder for the test project), its `docs/<Package>/README.md` and an entry in the package list of the root `README.md`.
- `src/Directory.Build.props` holds the properties shared by all projects, including the common release `<Version>` of all `Fusonic.Extensions.*` packages.
- `src/Directory.Packages.props` holds the versions of all referenced NuGet packages (central package management). **Never** add a `Version` attribute to `<PackageReference>` entries in `.csproj` files.
- Every `.cs` file starts with the Fusonic copyright/MIT license header used in the existing files.
- .NET 10, nullable reference types and implicit usings are enabled. Use primary constructors where they fit.
- Build output goes to `artifacts/` (e.g. `artifacts/bin/<Project>/debug`), not to `bin/` next to the project.

## Build

- The build must produce **no warnings**: CI fails on them (`dotnet-check-warnings`), and code style is enforced in the build via `.editorconfig`. Run `dotnet build Extensions.slnx` before finishing.
- XML documentation files are generated. Missing docs are allowed, but invalid ones (e.g. an unresolvable `cref`) are warnings. Document public extension methods and anything whose usage is not obvious.

## Dependency injection

Every package that registers services must support both containers:

- **SimpleInjector**: extension methods on `Container` in `ContainerExtensions`.
- **Microsoft.Extensions.DependencyInjection**: extension methods on `IServiceCollection` in `ServiceCollectionExtensions`. Use Scrutor only where plain Microsoft DI is not enough (decorators, assembly scanning).

Rules:

- Keep the logic container independent. Where a class needs the container, put the logic in a base class and add one implementation per container (e.g. `JobProcessorBase`, `JobProcessor` and `ServiceProviderJobProcessor` in Hangfire).
- Prefix Microsoft DI specific types with `ServiceProvider` (e.g. `ServiceProviderMediator`, `ServiceProviderTestFixture`).
- Services resolved by SimpleInjector must have a single public constructor. SimpleInjector cannot resolve `IServiceProvider`.
- Scrutor only decorates services registered before the `Decorate` call. Document the required registration order.
- Where a package only uses a container for optional registration helpers, reference it with `PrivateAssets="all"` so consumers are not forced to use it.
- Test both registrations and document both in the package's README.
- Exception: Mediator has no `ServiceCollectionExtensions`; with Microsoft DI, consumers register `ServiceProviderMediator` and scan handlers with Scrutor (see `docs/Mediator/README.md`).

## Documentation

The package docs are integration guides for consumers who only see the NuGet page. Update them with every change to the public API or its usage, and verify every statement against the code. Each doc:

- Names the NuGet package(s) right under the title and the namespaces to import.
- Lists prerequisites: framework services the package resolves (e.g. MVC, localization, `IHttpClientFactory`, `IUserAccessor`), SimpleInjector cross-wiring, and packages the consumer must reference because ours use `PrivateAssets="all"` (SimpleInjector, Scrutor).
- Shows registration for SimpleInjector and Microsoft DI, including the required order when decorators are involved, and keeps it consistent with the docs of packages it is combined with (e.g. Mediator and Hangfire).
- Uses complete samples that compile against the current code (correct names, signatures, `await`s). Prefer the test projects as the source of truth for working setups.
- Documents defaults and behaviour that are not obvious from signatures (e.g. timing, validation, what is silently skipped). No exhaustive API listings.
- Uses only in-page anchors or absolute `https://github.com/fusonic/dotnet-extensions/blob/main/...` links: relative links break on nuget.org. Keep the table of contents in sync.

## Tests

- Stack: xUnit v3, **AwesomeAssertions** (not FluentAssertions) and **NSubstitute**.
- Use the base classes from `Fusonic.Extensions.UnitTests` (`DependencyInjectionUnitTest<TFixture>`, `ServiceProviderTestFixture`, `SimpleInjectorTestFixture`) for tests needing DI, and `DatabaseUnitTest` from `UnitTests.EntityFrameworkCore` for database tests.
- Structure test methods with `// Arrange`, `// Act`, `// Assert` comments where the test is longer than a few lines.
- The runner is xUnit v3 via Microsoft.Testing.Platform (`global.json`). Run tests with `dotnet test` from the test project directory, or `dotnet test --solution Extensions.slnx` for all.
- `dotnet test --filter ...` (VSTest syntax) is silently ignored and runs all tests. To run a subset, build and run the test executable directly with xUnit v3 arguments, e.g. `artifacts/bin/<TestProject>/debug/<AssemblyName>.exe -class "<Namespace>.<TestClass>"` (or `-method`, `-list classes`, `--help`).
- Database tests (Hangfire, `UnitTests.EntityFrameworkCore.*`) start PostgreSQL/SQL Server and the Email tests start Mailpit via Testcontainers, so **Docker must be running**. Locally the database containers are reused across runs; in CI they run as services instead.
- A reused container keeps its databases and does not pick up schema changes (e.g. changed test entities or migrations). Remove the containers so the next test run recreates them:

  ```bash
  docker rm -f test.npgsql.extensions.unittests test.mssql.extensions.unittests test.npgsql.extensions.hangfire
  ```

## Merge requests

- Titles use one of these conventional commit prefixes: `feature`, `revert`, `fix`, `improvement`, `docs`, `cs`, `ci`, `infra`, `refactor`, `chore`, `perf`, `security`, `test`, `misc`. The first word after the colon is capitalized.
- When a ticket number is known, include it in the prefix, like `fix(#<ticket-number>): <title>`, and end the description with `Closes #<ticket-number>`.
- Use `.gitlab/merge_request_templates/Default.md` and replace all placeholders.
- Descriptions created or updated by an agent include a `## Review` section listing the review points as individual markdown checkboxes.
