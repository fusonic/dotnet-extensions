using FluentAssertions;
using NSubstitute;

namespace Example.Lib.Tests;

public class AddSomethingHandlerTests : TestBase
{
    public AddSomethingHandlerTests(TestFixture fixture) : base(fixture)
    { }

    [Fact]
    public void Handle_AddsSomething()
    {
        // Arrange
        // Set the test scoped ISomeService to always return number + 42
        GetInstance<ISomeService>().Calculate(0).ReturnsForAnyArgs(ci => ci.Arg<int>() + 42);

        // Act
        // The AddSomethingHandler gets resolved using a fresh LifetimeScope
        var result = Scoped(() => GetInstance<AddSomethingHandler>().Handle(new AddSomething(3)));

        // Assert
        result.Should().Be(45);
    }

    [Fact]
    public void Handle_AddsSomethingElse()
    {
        // Act
        // No side effects from previous test as ISomeService was registered TestScoped
        var result = Scoped(() => GetInstance<AddSomethingHandler>().Handle(new AddSomething(3)));

        // Assert
        // The mock for ISomeService.Calculate() returns the default value of int, 0, as it isn't configured otherwise.
        result.Should().Be(0);
    }
}