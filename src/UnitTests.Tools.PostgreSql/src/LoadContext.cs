using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Fusonic.Extensions.UnitTests.Tools.PostgreSql
{
    internal class LoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver resolver;
        private readonly string rootDirectory;

        public LoadContext(string assemblyPath)
        {
            string fullPath = Path.GetFullPath(assemblyPath);
            rootDirectory = Path.GetDirectoryName(fullPath)!;
            resolver = new AssemblyDependencyResolver(rootDirectory);

            EntryAssembly = LoadFromAssemblyPath(fullPath)!;
            Resolving += LoadContext_Resolving;
        }

        public Assembly EntryAssembly { get; }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            var assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
                return LoadFromAssemblyPath(assemblyPath);

            return null!;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var libraryPath = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
                return LoadUnmanagedDllFromPath(libraryPath);

            return IntPtr.Zero;
        }

        private Assembly LoadContext_Resolving(AssemblyLoadContext loadContext, AssemblyName assemblyName)
        {
            string dll = assemblyName.Name + ".dll";
            var path = Path.Combine(rootDirectory, dll);
            if (File.Exists(path) && GetAssemblyName(path).FullName == assemblyName.FullName)
                return LoadFromAssemblyPath(path);

            return null!;
        }
    }
}