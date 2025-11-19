// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using DotNet.Testcontainers.Containers;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;
using Xunit.Sdk;
using Xunit.v3;

public abstract class TestContainerStartup : ITestPipelineStartup
{
    protected internal static bool IsCi { get; private set; }

    private readonly List<DockerContainer> containers = new();
    private readonly string defaultName;
    private readonly string nameSuffix;

    static TestContainerStartup()
    {
        IsCi = bool.TryParse(Environment.GetEnvironmentVariable("CI"), out var isCi) && isCi // GitLab and GitHub
            || bool.TryParse(Environment.GetEnvironmentVariable("TF_BUILD"), out var tfBuild) && tfBuild; // Azure DevOps
    }

    public TestContainerStartup()
    {
        defaultName = string.Join(".", this.GetType()
                                    .Namespace!
                                    .ToLowerInvariant()
                                    .Split('.')
                                    .Reverse());

        nameSuffix = IsCi ? $"-{Guid.NewGuid():n}" : "";
    }

    /// <summary>
    /// Starts a PostgreSQL container. PostgreSQL-Data is mounted as tmpfs to improve testing performance. Also see https://www.fusonic.net/de/blog/fusonic-test-with-databases-part-3 for more information.
    /// </summary>
    /// <param name="image">The Docker image to use. Default is "postgres:17".</param>
    /// <param name="configure">An optional action to further configure the PostgreSQL container.</param>
    /// <returns>The started PostgreSQL container.</returns>
    public async Task<PostgreSqlContainer> StartPostgreSql(string image = "postgres:17", Action<PostgreSqlBuilder>? configure = null)
    {
        var builder = new PostgreSqlBuilder()
            .ConfigureDefaults(image, GetName("postgres"))
            .WithTmpfsMount("/var/lib/postgresql/data")
            .WithTmpfsMount("/dev/shm");

        configure?.Invoke(builder);

        return await StartContainer(builder.Build());
    }

    /// <summary>
    /// Starts a MS SQL Server container.
    /// </summary>
    /// <param name="image">The Docker image to use. Default is "mcr.microsoft.com/mssql/server:2022-latest".</param>
    /// <param name="configure">An optional action to further configure the MS SQL Server container.</param>
    /// <returns>The started MS SQL Server container.</returns>
    public async Task<MsSqlContainer> StartMsSql(string image = "mcr.microsoft.com/mssql/server:2022-latest", Action<MsSqlBuilder>? configure = null)
    { 
        var builder = new MsSqlBuilder().ConfigureDefaults(image, GetName("mssql"));

        configure?.Invoke(builder);

        return await StartContainer(builder.Build());
    }

    private string GetName(string suffix) => $"{defaultName}.{suffix}{nameSuffix}";

    private async Task<T> StartContainer<T>(T container) where T : DockerContainer
    {
        containers.Add(container);
        await container.StartAsync();
        return container;
    }

    /// <inheritdoc/>
    public abstract ValueTask StartAsync(IMessageSink diagnosticMessageSink);

    /// <inheritdoc/>
    public virtual async ValueTask StopAsync()
    {
        if (IsCi)
        {
            foreach (var container in containers)
            {
                await container.StopAsync();
                await container.DisposeAsync();
            }
        }

        containers.Clear();
    }
}
