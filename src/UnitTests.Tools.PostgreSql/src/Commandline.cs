// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using CommandLine;

namespace Fusonic.Extensions.UnitTests.Tools.PostgreSql;

internal sealed class Commandline
{
    public static int ParseAndExecute(string[] args)
        => Parser.Default
                 .ParseArguments<CleanupOptions, DropOptions, TemplateOptions>(args)
                 .MapResult(
                      (CleanupOptions o) => o.Execute(),
                      (DropOptions o) => o.Execute(),
                      (TemplateOptions o) => o.Execute(),
                      _ => 1);
}