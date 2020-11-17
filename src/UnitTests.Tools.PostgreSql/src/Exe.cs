// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Fusonic.Extensions.UnitTests.Tools.PostgreSql
{
    //Shamelessly taken from dotnet-ef Microsoft.EntityFrameworkCore.Tools
    internal static class Exe
    {
        public static int Run(string executable, IReadOnlyList<string> args, string workingDirectory)
        {
            var arguments = ToArguments(args);

            var startInfo = new ProcessStartInfo
            {
                FileName = executable,
                Arguments = arguments,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory
            };

            var process = Process.Start(startInfo);
            if (process == null)
                throw new InvalidOperationException($"Process '{executable}' did not start.");

            process.WaitForExit();
            return process.ExitCode;
        }

        private static string ToArguments(IReadOnlyList<string> args)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < args.Count; i++)
            {
                if (i != 0)
                {
                    builder.Append(' ');
                }

                if (!args[i].Contains(' '))
                {
                    builder.Append(args[i]);

                    continue;
                }

                builder.Append('"');

                var pendingBackslashes = 0;
                for (var j = 0; j < args[i].Length; j++)
                {
                    switch (args[i][j])
                    {
                        case '\"':
                            if (pendingBackslashes != 0)
                            {
                                builder.Append('\\', pendingBackslashes * 2);
                                pendingBackslashes = 0;
                            }
                            builder.Append("\\\"");
                            break;

                        case '\\':
                            pendingBackslashes++;
                            break;

                        default:
                            if (pendingBackslashes != 0)
                            {
                                if (pendingBackslashes == 1)
                                {
                                    builder.Append('\\');
                                }
                                else
                                {
                                    builder.Append('\\', pendingBackslashes * 2);
                                }

                                pendingBackslashes = 0;
                            }

                            builder.Append(args[i][j]);
                            break;
                    }
                }

                if (pendingBackslashes != 0)
                {
                    builder.Append('\\', pendingBackslashes * 2);
                }

                builder.Append('"');
            }

            return builder.ToString();
        }
    }
}
