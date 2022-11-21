using FluentAssertions;
using Fusonic.Extensions.AspNetCore.Validation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NSubstitute;
using Xunit;

namespace Fusonic.Extensions.AspNetCore.Tests.Validation;

public class RequestValidationDecoratorTests
{
    private readonly IObjectModelValidator modelValidator;
    private readonly IRequestHandler<Model, int> requestHandler;

    private readonly RequestValidationDecorator<Model, int> decorator;

    public RequestValidationDecoratorTests()
    {
        modelValidator = Substitute.For<IObjectModelValidator>();
        requestHandler = Substitute.For<IRequestHandler<Model, int>>();
        requestHandler.Handle(default!, default).ReturnsForAnyArgs(42);

        decorator = new RequestValidationDecorator<Model, int>(requestHandler, modelValidator);
    }

    [Fact]
    public async Task ModelValidated_NoErrors_CallsHandler()
    {
        await decorator.Handle(new Model("Hi"), CancellationToken.None);

        await requestHandler.Received(1).Handle(Arg.Is<Model>(m => m.Name == "Hi"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ModelValidated_NoErrors_ReturnsHandlerResult()
    {
        var result = await decorator.Handle(new Model("Hi"), CancellationToken.None);
        result.Should().Be(42);
    }

    [Fact]
    public async Task ModelValidated_HasErrors_ThrowsException_WithErrorDetails()
    {
        SetError();

        var act = () => decorator.Handle(new Model("Hi"), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<RequestValidationException>();
        ex.Which.Errors.Should().ContainKeys("FooKey", "BarKey");
        ex.Which.Errors["FooKey"].Should().BeEquivalentTo("Foo error 1", "Foo error 2");
        ex.Which.Errors["BarKey"].Should().ContainSingle("Bar error 1");
    }

    [Fact]
    public async Task ModelValidated_HasErrors_DoesNotCallHandler()
    {
        SetError();

        try
        {
            await decorator.Handle(new Model("Hi"), CancellationToken.None);
        }
        catch (RequestValidationException) { }

        await requestHandler.Received(0).Handle(Arg.Any<Model>(), Arg.Any<CancellationToken>());
    }

    private void SetError() => modelValidator.Validate(Arg.Do<ActionContext>(ctx =>
    {
        ctx.ModelState.AddModelError("FooKey", "Foo error 1");
        ctx.ModelState.AddModelError("FooKey", "Foo error 2");
        ctx.ModelState.AddModelError("BarKey", "Bar error 1");
    }), Arg.Any<ValidationStateDictionary?>(), Arg.Any<string>(), Arg.Any<object?>());

    public record Model(string Name) : IRequest<int>;
}