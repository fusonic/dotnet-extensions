# Common

Package: `Fusonic.Extensions.Common`

- [Common](#common)
  - [PropertyUtil](#propertyutil)
  - [PathUtil](#pathutil)
  - [TempFileStream](#tempfilestream)
  - [TransactionScopeHandler](#transactionscopehandler)
  - [Entities](#entities)
  - [IUserAccessor](#iuseraccessor)

This project contains general, framework independent utilities and abstractions.

## PropertyUtil

Namespace: `Fusonic.Extensions.Common.Reflection`

Utility to get the names and types from property expressions.

Example:

```cs
public class RandomClass
{
    public bool Flag { get; set; }
    public string Title { get; set; } = "";
}
```

```cs
PropertyUtil.GetName<RandomClass>(c => c.Flag);      // Returns "Flag"
PropertyUtil.GetType<RandomClass>(c => c.Title);     // Returns typeof(string)
PropertyUtil.GetName<RandomClass>(c => c.GetType()); // Throws ArgumentException - .GetType() is no valid property expression
```

`GetName<T, TResult>(Expression<Func<T, TResult>>)` avoids boxing value type properties.

## PathUtil

Namespace: `Fusonic.Extensions.Common.IO`

Utility for paths. Does not try to replicate `System.IO.Path`, but adds some helpers that `Path` doesn't.

`Path.GetInvalidFileNameChars()` returns different values based on the OS, while `PathUtil` uses a fixed set (the one of Windows, a superset of the others). This is useful e.g. when generating a filename for a download.

```cs
PathUtil.RemoveInvalidFilenameChars("a:b?.txt"); // "ab.txt"
PathUtil.RemoveInvalidPathChars(path);
PathUtil.HasInvalidFilenameChar(filename);       // bool
PathUtil.HasInvalidPathChar(path);               // bool
PathUtil.IsInvalidFilenameChar(c);               // bool
PathUtil.IsInvalidPathChar(c);                   // bool
```

`RemoveInvalidFilenameChars` also removes all chars removed by `RemoveInvalidPathChars`.

## TempFileStream

Namespace: `Fusonic.Extensions.Common.IO`

This creates a temporary file that gets automatically deleted when it is closed. Example:

```cs
await using (var fs = new TempFileStream())
{
    // Write content to file
    // Upload file
}
// File was deleted at this point.
```

The constructor takes an optional `bufferSize` (default 4096).

## TransactionScopeHandler

Namespace: `Fusonic.Extensions.Common.Transactions`

`ITransactionScopeHandler` / `TransactionScopeHandler` provide an easy way to run code within a `TransactionScope` (with async flow enabled). The transaction options are:
```cs
IsolationLevel = IsolationLevel.ReadCommitted
Timeout = TransactionManager.MaximumTimeout
```

Register it in your container:

```cs
// Microsoft.Extensions.DependencyInjection
services.AddSingleton<ITransactionScopeHandler, TransactionScopeHandler>();

// SimpleInjector
container.RegisterSingleton<ITransactionScopeHandler, TransactionScopeHandler>();
```

With SimpleInjector, `RegisterMediator` from `Fusonic.Extensions.Mediator` already registers it when transactional decorators are enabled (the default). The methods are `virtual`, so you can derive from `TransactionScopeHandler` to change the behavior.

Example:
```cs
using System.Transactions;

// Runs async method FooMethod in a transaction. An ambient transaction gets used if it exists, otherwise a new one will be created.
await transactionScopeHandler.RunInTransactionScope(FooMethod);

// Runs the lambda in a new transaction.
var result = await transactionScopeHandler.RunInTransactionScope(async () => { await BarMethod(); return 42; }, TransactionScopeOption.RequiresNew);

// Runs LogMethod and suppresses any ambient transaction.
await transactionScopeHandler.RunInTransactionScope(LogMethod, TransactionScopeOption.Suppress);

// Shortcut for suppressing the transaction
await transactionScopeHandler.SuppressTransaction(LogMethod);
```

## Entities

Namespace: `Fusonic.Extensions.Common.Entities`

- `IEntity` / `IEntity<TId>` (with an `Id` property) mark your entities. They are used e.g. by the query extensions of `Fusonic.Extensions.EntityFrameworkCore`.
- `EntityNotFoundException` signals a missing entity. It has the optional properties `EntityType` and `EntityId`, e.g. `throw new EntityNotFoundException(typeof(User), id);`.

## IUserAccessor

Namespace: `Fusonic.Extensions.Common.Security`

Abstraction to access the current user (`ClaimsPrincipal User` and `bool TryGetUser(out ClaimsPrincipal user)`). `Fusonic.Extensions.AspNetCore` provides `HttpContextUserAccessor` as implementation; `Fusonic.Extensions.Hangfire` requires one to be registered.
