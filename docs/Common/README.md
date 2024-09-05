# Common

- [Common](#common)
  - [PropertyUtil](#propertyutil)
  - [PathUtil](#pathutil)

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

## TempFileStream

This creates a temporary file that gets automatically deleted when it is closed. Example:

```cs
await using (var fs = new TempFileStream())
{
    // Write content to file
    // Upload file
}
// File was deleted at this point.
```