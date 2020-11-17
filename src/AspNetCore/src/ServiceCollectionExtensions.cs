// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> Registers all classes implementing {T} within the given assemblies. </summary>
        public static void AddAll<T>(this IServiceCollection services, IEnumerable<Assembly> assemblies, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException("Type must be an interface.");

            var implementingTypes = assemblies.SelectMany(a => a.DefinedTypes)
                                              .Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
                                              .ToList();

            foreach (var type in implementingTypes)
            {
                services.Add(new ServiceDescriptor(typeof(T), type, lifetime));
            }
        }
    }
}
