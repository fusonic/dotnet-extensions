// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Aspire.Hosting.ApplicationModel;
using Fusonic.Extensions.Aspire.Hosting;

// Aspire's convention for AppHost extensions: available in the AppHost without a using.
#pragma warning disable IDE0130
namespace Aspire.Hosting;
#pragma warning restore IDE0130

/// <summary>
/// Labels the AppHost's containers like Docker Compose does, so Docker Desktop and other tools show them grouped as one
/// stack instead of a flat list.
/// </summary>
public static class ComposeStackExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        /// <summary>
        /// Groups all containers of the AppHost (including child resources like pgAdmin) as one Compose stack. The service
        /// name is the resource name, unless set with <see cref="WithComposeService{T}"/>.
        /// </summary>
        /// <param name="projectName">
        /// The stack's name. Defaults to the AppHost's name without <c>.AppHost</c>, e.g. <c>myapp</c> for
        /// <c>MyApp.AppHost</c>. Normalized to what Compose allows (lowercase letters, digits, <c>-</c>, <c>_</c>).
        /// </param>
        /// <remarks>
        /// Changing the labels changes a persistent container's configuration, so Aspire recreates it once; its volumes stay.
        /// </remarks>
        public IDistributedApplicationBuilder UseComposeStack(string? projectName = null)
        {
            var project = ComposeNames.Normalize(projectName ?? ComposeNames.DefaultProjectName(builder.Environment.ApplicationName));
            if (project.Length == 0)
                throw new ArgumentException("The Compose project name must contain at least one letter or digit.", nameof(projectName));

            if (!builder.ExecutionContext.IsRunMode)
                return builder;

            builder.Eventing.Subscribe<BeforeStartEvent>((@event, _) =>
            {
                ApplyLabels(@event.Model, project);
                return Task.CompletedTask;
            });

            return builder;
        }
    }

    extension<T>(IResourceBuilder<T> builder) where T : ContainerResource
    {
        /// <summary>Sets the container's service name in the Compose stack (default: the resource name).</summary>
        public IResourceBuilder<T> WithComposeService(string serviceName)
        {
            var service = ComposeNames.Normalize(serviceName);
            if (service.Length == 0)
                throw new ArgumentException("The Compose service name must contain at least one letter or digit.", nameof(serviceName));

            return builder.WithAnnotation(new ComposeServiceAnnotation(service), ResourceAnnotationMutationBehavior.Replace);
        }

        /// <summary>Leaves the container out of the Compose stack.</summary>
        public IResourceBuilder<T> WithoutComposeStack() =>
            builder.WithAnnotation(new ComposeServiceAnnotation(null), ResourceAnnotationMutationBehavior.Replace);
    }

    internal static void ApplyLabels(DistributedApplicationModel model, string project)
    {
        foreach (var container in model.Resources.OfType<ContainerResource>())
        {
            var annotation = container.Annotations.OfType<ComposeServiceAnnotation>().LastOrDefault();
            if (annotation is { ServiceName: null })
                continue;

            var service = annotation?.ServiceName ?? ComposeNames.Normalize(container.Name);
            container.Annotations.Add(new ContainerRuntimeArgsCallbackAnnotation(args =>
            {
                args.Add("--label");
                args.Add($"com.docker.compose.project={project}");
                args.Add("--label");
                args.Add($"com.docker.compose.service={service}");
                args.Add("--label");
                args.Add("com.docker.compose.version=2.0.0");
            }));
        }
    }
}
