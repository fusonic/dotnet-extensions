using Fusonic.Extensions.UnitTests;
using Fusonic.Extensions.UnitTests.SimpleInjector;
using NSubstitute;
using SimpleInjector;

namespace Example.Lib.Tests;

public class TestFixture : UnitTestFixture
{
    protected sealed override void RegisterCoreDependencies(Container container)
    {
        container.Register<AddSomethingHandler>();
        container.RegisterTestScoped(() => Substitute.For<ISomeService>());
    }
}