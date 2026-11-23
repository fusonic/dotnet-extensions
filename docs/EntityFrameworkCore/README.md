# EntityFrameworkCore

[![NuGet](https://img.shields.io/nuget/v/Fusonic.Extensions.EntityFrameworkCore.svg?style=flat-square&logo=nuget&label=Fusonic.Extensions.EntityFrameworkCore)](https://www.nuget.org/packages/Fusonic.Extensions.EntityFrameworkCore/) ![.NET 11](https://img.shields.io/badge/.NET-11.0-512BD4?style=flat-square&logo=dotnet)

```bash
dotnet add package Fusonic.Extensions.EntityFrameworkCore
```

- [EntityFrameworkCore](#entityframeworkcore)
  - [Query extensions](#query-extensions)
  - [HasIndex](#hasindex)
  - [Conventions](#conventions)

All types are in the namespace `Fusonic.Extensions.EntityFrameworkCore`.

**At a glance**

| Feature | What it does |
|---|---|
| [Query extensions](#query-extensions) | Query entities that must exist; throw `EntityNotFoundException` instead of returning `null` |
| [HasIndex](#hasindex) | Create a composite index from property expressions |
| [Conventions](#conventions) | `NoActionDeleteConvention`: `NO ACTION` instead of `CASCADE` on foreign keys |

## Query extensions

Extension methods for querying entities that must exist. They throw an `EntityNotFoundException` (from `Fusonic.Extensions.Common.Entities`) instead of returning `null` or throwing an `InvalidOperationException`. Methods taking an id require the entity to implement `IEntity<TId>`, `IsRequired`/`FindRequiredAsync` require `IEntity` (see the [Common documentation](https://github.com/fusonic/dotnet-extensions/blob/main/docs/Common/README.md#entities)).

```cs
User a = await dbContext.Users.FindRequiredAsync(userId, cancellationToken);
User b = await dbContext.Users.SingleRequiredAsync(userId, cancellationToken);
User c = await dbContext.Users.Where(u => u.IsActive).SingleRequiredAsync(u => u.Email == email, cancellationToken);
User d = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email).IsRequiredAsync();
User e = maybeUser.IsRequired(); // User? -> User

await dbContext.Users.IsRequiredAsync(userId, cancellationToken);                  // only checks existence
await dbContext.Users.Where(u => u.Email == email).IsRequiredAsync(cancellationToken); // only checks existence (AnyAsync)
bool exists = await dbContext.Users.ExistsAsync(userId, cancellationToken);
```

## HasIndex

`HasIndex` with property expressions, which creates one (composite) index over all given properties:

```cs
builder.Entity<User>().HasIndex(u => u.LastName, u => u.FirstName);
```

## Conventions
Provides a more sensible delete convention than EF Core's [`CascadeDeleteConvention`](https://github.com/dotnet/efcore/blob/main/src/EFCore/Metadata/Conventions/CascadeDeleteConvention.cs). `NoActionDeleteConvention` sets `NO ACTION` instead of `CASCADE` on required foreign keys, and `ClientSetNull` on optional ones. Deleting a principal then fails as long as dependent rows exist, so the dependent rows must be deleted explicitly.

With the default EF Core behavior, it is easy to create bugs that wouldn't be caught automatically by your testing. For example when referencing a `User` in a `History` and deleting the `User`, the `History` would be deleted as well. This is most likely not the desired behavior.
Therefore, if such a behavior is intended it should either be implemented
- by manually deleting all references in code
- by explicitly overriding the delete behavior on your foreign key
- by setting the foreign key as nullable

See https://github.com/dotnet/efcore/issues/27479, https://learn.microsoft.com/en-us/ef/core/saving/cascade-delete and https://www.postgresql.org/docs/current/ddl-constraints.html#DDL-CONSTRAINTS-FK for more information.

To use this, add this to your `DbContext`:

```cs
using Fusonic.Extensions.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
{
    configurationBuilder.Conventions.Remove<CascadeDeleteConvention>();
    configurationBuilder.Conventions.Add(static _ => new NoActionDeleteConvention());
}
```

---

<sub>Part of [Fusonic Extensions](https://github.com/fusonic/dotnet-extensions) · [All packages](https://github.com/fusonic/dotnet-extensions#-packages) · MIT License</sub>
