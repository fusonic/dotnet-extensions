// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.Aspire.Hosting.Tests;

public class ComposeStackTests
{
    [Fact]
    public async Task UseComposeStack_LabelsAllContainers_WithResourceNameAsService()
    {
        var builder = CreateBuilder();
        builder.UseComposeStack("My Stack");
        builder.AddContainer("Db", "postgres");
        builder.AddContainer("cache", "valkey/valkey");

        var labels = await StartAndGetLabels(builder);

        labels["Db"].Should().BeEquivalentTo(Labels("my-stack", "db"));
        labels["cache"].Should().BeEquivalentTo(Labels("my-stack", "cache"));
    }

    [Fact]
    public async Task WithComposeService_OverridesServiceName()
    {
        var builder = CreateBuilder();
        builder.UseComposeStack("stack");
        builder.AddContainer("db", "postgres").WithComposeService("postgres");

        var labels = await StartAndGetLabels(builder);

        labels["db"].Should().BeEquivalentTo(Labels("stack", "postgres"));
    }

    [Fact]
    public async Task WithoutComposeStack_LeavesContainerOut()
    {
        var builder = CreateBuilder();
        builder.UseComposeStack("stack");
        builder.AddContainer("db", "postgres").WithoutComposeStack();

        var labels = await StartAndGetLabels(builder);

        labels["db"].Should().BeEmpty();
    }

    [Fact]
    public async Task WithoutUseComposeStack_NoLabels()
    {
        var builder = CreateBuilder();
        builder.AddContainer("db", "postgres").WithComposeService("postgres");

        var labels = await StartAndGetLabels(builder);

        labels["db"].Should().BeEmpty();
    }

    [Fact]
    public void UseComposeStack_NameWithoutLettersOrDigits_Throws()
    {
        var builder = CreateBuilder();

        var act = () => builder.UseComposeStack("..");

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("MyApp.AppHost", "MyApp")]
    [InlineData("MyApp.apphost", "MyApp")]
    [InlineData("MyApp", "MyApp")]
    [InlineData(".AppHost", ".AppHost")]
    public void DefaultProjectName_StripsAppHostSuffix(string applicationName, string expected)
        => ComposeNames.DefaultProjectName(applicationName).Should().Be(expected);

    [Theory]
    [InlineData("RebuildYourSets", "rebuildyoursets")]
    [InlineData("My.App Host", "my-app-host")]
    [InlineData("-_rys-aspire_-", "rys-aspire")]
    [InlineData("äöü", "")]
    public void Normalize_KeepsWhatComposeAllows(string name, string expected)
        => ComposeNames.Normalize(name).Should().Be(expected);

    /// <summary>
    /// Aspire's own <see cref="BeforeStartEvent"/> handlers validate the DCP and dashboard paths, which only the AppHost
    /// SDK sets; nothing is started here, so any value does.
    /// </summary>
    private static IDistributedApplicationBuilder CreateBuilder()
    {
        var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions { DisableDashboard = true });
        builder.Configuration["DcpPublisher:CliPath"] = "dcp";
        builder.Configuration["DcpPublisher:DashboardPath"] = "dashboard";
        return builder;
    }

    private static string[] Labels(string project, string service) =>
    [
        "--label", $"com.docker.compose.project={project}",
        "--label", $"com.docker.compose.service={service}",
        "--label", "com.docker.compose.version=2.0.0",
    ];

    /// <summary>Raises <see cref="BeforeStartEvent"/> like a start would, without starting anything.</summary>
    private static async Task<Dictionary<string, List<object>>> StartAndGetLabels(IDistributedApplicationBuilder builder)
    {
        await using var app = builder.Build();
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();
        await builder.Eventing.PublishAsync(new BeforeStartEvent(app.Services, model), TestContext.Current.CancellationToken);

        var result = new Dictionary<string, List<object>>();
        foreach (var container in model.Resources.OfType<ContainerResource>())
        {
            var args = new List<object>();
            var context = new ContainerRuntimeArgsCallbackContext(args, TestContext.Current.CancellationToken);
            foreach (var annotation in container.Annotations.OfType<ContainerRuntimeArgsCallbackAnnotation>())
            {
                await annotation.Callback(context);
            }

            result[container.Name] = args;
        }

        return result;
    }
}
