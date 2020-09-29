# Common

- [Common](#common)
  - [PropertyUtil](#propertyutil)
  - [PathUtil](#pathutil)
  - [Transactions](#transactions)

This project contains general, framework independent utilities and abstractions.

## PropertyUtil

Utility to get the names and types from property expressions.

Example:

```cs
public class RandomClass
{
    public bool Flag { get; set; }
    public string Title { get; set; }
}
```

```cs
PropertyUtil.GetName<RandomClass>(c => c.Flag) //Returns "Flag"
PropertyUtil.GetType<RandomClass>(c => c.Title) //Returns typeof(string)
PropertyUtil.GetName<RandomClass>(c => c.GetType()) //Throws ArgumentException - .GetType() is no valid property expression
```

## PathUtil

Utility for paths. Does not try to replicate `System.IO.Path`, but adds some helpers that `Path` doesn't.

Currently this only has methods for removing invalid chars from paths and filenames, as `Path.GetInvalidFileNameChars()` returns different values based on the OS, while `PathUtil` uses a fixed set. This may be for example required when generating a filename for a download.

## Transactions

If you need to use trasactions, use the transaction scope handler to run your code in one. You normally don't need to care about transactions, as the MediatR-Pipeline and Hangfire-Jobs have handlers for that (`TransactionalDecorator`, `TransactionalJobHandler`). See the [Hangfire](../Hangfire/README.md) or [MediatR](#mediatr) docs for details.

```cs
Container.RegisterSingleton<ITransactionScopeHandler, TransactionScopeHandler>();
```
