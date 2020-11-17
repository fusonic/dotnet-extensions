// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediatR.Pipeline;
using NSubstitute;
using SimpleInjector;
using Xunit;

namespace Fusonic.Extensions.Validation.Tests
{
    public class ValidationPreProcessorTests
    {
        private readonly IMediator mediator;
        private readonly IRequestHandler<TestCommand, Unit> handler;
        private readonly IValidator<TestCommand> validator;

        public ValidationPreProcessorTests()
        {
            var container = new Container();
            container.RegisterSingleton<IMediator, Mediator>();
            container.Collection.Register(typeof(IPipelineBehavior<,>), new[]
            {
                typeof(RequestPreProcessorBehavior<,>),
                typeof(RequestPostProcessorBehavior<,>),
            });
            container.RegisterInstance(new ServiceFactory(container.GetInstance));
            container.Collection.Register(typeof(IRequestPreProcessor<>), typeof(TestCommand).Assembly);
            container.Collection.Register(typeof(IRequestPostProcessor<,>), typeof(TestCommand).Assembly);

            container.RegisterValidators(new Assembly[0]);

            handler = Substitute.For<IRequestHandler<TestCommand, Unit>>();
            container.RegisterInstance(handler);
            validator = Substitute.For<IValidator<TestCommand>>();
            container.Collection.AppendInstance(typeof(IValidator<>), validator);
            container.Verify();

            mediator = container.GetInstance<IMediator>();
        }

        [Fact]
        public async Task PerformsValidation()
        {
            await Assert.ThrowsAsync<ObjectValidationException>(() => mediator.Send(new TestCommand()));
        }

        [Fact]
        public async Task DoesntCallHandler()
        {
            await Assert.ThrowsAsync<ObjectValidationException>(() => mediator.Send(new TestCommand()));

            await handler.DidNotReceiveWithAnyArgs().Handle(Arg.Any<TestCommand>(), CancellationToken.None);
        }

        [Fact]
        public async Task DoesntCallValidatorOnInvalidObjectValidation()
        {
            await Assert.ThrowsAsync<ObjectValidationException>(() => mediator.Send(new TestCommand()));

            await validator.DidNotReceiveWithAnyArgs().ValidateAsync(Arg.Any<TestCommand>(), CancellationToken.None);
        }

        [Fact]
        public async Task CallsValidatorWhenObjectValidationPasses()
        {
            validator.ValidateAsync(Arg.Any<TestCommand>(), CancellationToken.None).ReturnsForAnyArgs(ValidationResult.Error(0));

            await Assert.ThrowsAsync<ObjectValidationException>(() => mediator.Send(new TestCommand { Test = "valid" }));

            await validator.ReceivedWithAnyArgs(1).ValidateAsync(Arg.Any<TestCommand>(), CancellationToken.None);
        }

        [Fact]
        public async Task CallsHandlerWhenAllValidationsPass()
        {
            validator.ValidateAsync(Arg.Any<TestCommand>(), CancellationToken.None).ReturnsForAnyArgs(ValidationResult.Success());

            await mediator.Send(new TestCommand { Test = "valid" });

            await handler.ReceivedWithAnyArgs(1).Handle(Arg.Any<TestCommand>(), CancellationToken.None);
        }
    }

    public class TestCommand : IRequest
    {
        [Required]
        public string? Test { get; set; }
    }
}