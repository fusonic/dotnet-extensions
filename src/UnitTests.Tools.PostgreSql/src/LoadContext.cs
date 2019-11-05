using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace Fusonic.Extensions.UnitTests.Tools.PostgreSql
{
    //TODO: Revisit this with .NET Core 3.0
    //https://github.com/dotnet/coreclr/issues/23757#issuecomment-489261268
    public class LoadContext : AssemblyLoadContext
    {
        private readonly string rootDirectory;
        private readonly string nugetPackageDirectory;
            
        public LoadContext(string assemblyPath)
        {
            string fullPath = Path.GetFullPath(assemblyPath);
            rootDirectory = Path.GetDirectoryName(fullPath);
            EntryAssembly = LoadFromAssemblyPath(fullPath);
                
            var packageDirectory = Environment.GetEnvironmentVariable("NUGET_PACKAGES");

            if (!string.IsNullOrEmpty(packageDirectory))
                nugetPackageDirectory = packageDirectory;
            else
            {
                string basePath = Environment.GetEnvironmentVariable(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "USERPROFILE" : "HOME");
                nugetPackageDirectory = Path.Combine(basePath, ".nuget", "packages");
            }

            Resolving += LoadContext_Resolving;
        }


        private Assembly LoadContext_Resolving(AssemblyLoadContext loadContext, AssemblyName assemblyName)
        {
            string dll = assemblyName.Name + ".dll";
            var asm = LoadFrom(Path.Combine(rootDirectory, dll));
            if (asm != null)
                return asm;

            var nugetPath = Path.Combine(nugetPackageDirectory, assemblyName.Name.ToLowerInvariant(), assemblyName.Version.ToString(3), "lib", "netstandard2.0", dll);
            asm = LoadFrom(nugetPath);
            if (asm != null)
                return asm;

            return null!;

            Assembly? LoadFrom(string path)
            {
                if (File.Exists(path) && GetAssemblyName(path).FullName == assemblyName.FullName)
                    return LoadFromAssemblyPath(path);

                return null;
            }
        }

        public Assembly EntryAssembly { get; }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null!;
        }
    }
}