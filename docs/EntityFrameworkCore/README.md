# EntityFrameworkCore

- [EntityFrameworkCore](#entityframeworkcore)
  - [Extensions](#extensions)
  - [Conventions](#conventions)

## Extensions
Provides a set of extension methods to make common querying tasks easier. See `QueryableExtensions.cs`

## Conventions
Provides a more sensible delete convention than EF Core's [`CascadeDeleteConvention`](https://github.com/dotnet/efcore/blob/main/src/EFCore/Metadata/Conventions/CascadeDeleteConvention.cs). `NoActionDeleteConvention` sets `NO ACTION` on your foreign key per default instead of `CASCADE`, which forces all dependent foreign keys to also be deleted by the user.

With the default EF Core behavior, it is easy to create bugs that wouldn't be caught automatically by your testing. For example when referencing a `User` in a `History` and deleting the `User`, the `History` would be deleted as well. This is most likely not the desired behavior. 
Therefore, if such a behavior is intended it should either be implemented
- by manually deleting all references in code
- by explicitly overriding the delete behavior on your foreign key
- by setting the foreign key as nullable

See https://github.com/dotnet/efcore/issues/27479, https://learn.microsoft.com/en-us/ef/core/saving/cascade-delete and https://www.postgresql.org/docs/current/ddl-constraints.html#DDL-CONSTRAINTS-FK for more information.

To use this, add this to your `DbContext`:

```cs
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
{
     configurationBuilder.Conventions.Remove<CascadeDeleteConvention>();
     configurationBuilder.Conventions.Add(static _ => new NoActionDeleteConvention());
}
```
