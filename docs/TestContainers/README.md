# Test containers

EXPERIMENTAL. APIs might change any time as this library is currently only targeted for internal testing.

`Fusonic.Extensions.TestContainers` provides XUnit v3 startup classes that start some commonly used test containers.

When running the tests within a GitLab, GitHub or Azure DevOps-CI pipeline, the test containers get random names and get cleaned up after test execution.

Outside of a (known) CI pipeline, the test containers get stable names and do not get cleaned up after test execution. This is intended to speed up repeated test execution in local dev environments.

Example usage:
```cs
// In your test Project
using MyProject.Tests;
using Microsoft.Data.SqlClient;
using Xunit.Sdk;
using Xunit.v3;

[assembly: TestPipelineStartup(typeof(TestStartup))]

namespace MyProject.Tests;

public class TestStartup : TestContainerStartup
{
    public static string ConnectionString { get; private set; } = null!;

    public override async ValueTask StartAsync(IMessageSink diagnosticMessageSink)
    {
        var container = await StartMsSql();
        ConnectionString = new SqlConnectionStringBuilder(container.GetConnectionString())
        {
            InitialCatalog = "testtemplate"
        }.ConnectionString;
    }
}
```

## Running in GitLab

In order to use TestContainers in GitLab, start `docker:dind-rootless` as a service. When running rootless dind, you also must set `TESTCONTAINERS_RYUK_DISABLED` to `true`, as there is no `docker.sock` available. Ryuk is responsible for cleaning up the test containers, even when test jobs get cancelled. Disabling it should be safe though, as the containers started within `docker:dind` get cleaned up anyway.

Example:
```yaml
variables:
  DOCKER_VERSION: "29"
  DOCKER_BUILDKIT: 1
  DOCKER_HOST: tcp://docker:2376
  DOCKER_TLS_CERTDIR: "/certs/${CI_JOB_ID}"
  DOCKER_TLS_VERIFY: 1
  DOCKER_CERT_PATH: "/certs/${CI_JOB_ID}/client"
  TESTCONTAINERS_RYUK_DISABLED: "true" # Disable Ryuk as we're running in dind-rootless and there is no docker.sock available.

dotnet:test:
  services:
    - docker:${DOCKER_VERSION}-dind-rootless
  script:
    - echo "Running Tests..."
```