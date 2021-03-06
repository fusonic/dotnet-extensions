// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.UnitTests.Tools.PostgreSql
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length > 0 && args[0] == TemplateCreator.Verb)
                return TemplateCreator.Run(args);

            //Cannot continue with statics or in the same class here as otherwise references to other libs (like CommandLineParser) get loaded, which may
            //not be available when running in a completely different load context of another project when creating templates.
            return new Commandline().ParseAndExecute(args);
        }
    }
}