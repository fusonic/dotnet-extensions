// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

internal static class ContainerBuilderExtensions
{
    extension<TBuilderEntity, TContainerEntity, TConfigurationEntity>(ContainerBuilder<TBuilderEntity, TContainerEntity, TConfigurationEntity> builder)
        where TBuilderEntity : ContainerBuilder<TBuilderEntity, TContainerEntity, TConfigurationEntity>, new()
        where TContainerEntity : IContainer
        where TConfigurationEntity : IContainerConfiguration
    {
        public TBuilderEntity ConfigureDefaults(string image, string name)
        {
            // Cleanup containers when running inside a CI pipeline
            // Developers might run the tests multiple times locally and should be able to reuse the same container
            return builder.WithImage(image)
                          .WithName(name)
                          .WithLabel("testcontainer", name)
                          .WithCleanUp(TestContainerStartup.IsCi)
                          .WithReuse(!TestContainerStartup.IsCi);
        }
    }
}