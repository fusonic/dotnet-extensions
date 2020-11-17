// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleInjector;
using Xunit;

namespace Fusonic.Extensions.Validation.Tests
{
    public class ContainerExtensionsTests
    {
        [Fact]
        public void RegisteresAllValidators()
        {
            var container = new Container();
            container.RegisterValidators(new []{typeof(TestValidator).Assembly});
            container.Verify();

            var validators = container.GetAllInstances<IValidator<TestObject>>();
            Assert.Equal(3, validators.Count());
        }

        [Fact]
        public void RegistersInRightOrder()
        {
            var container = new Container();
            container.RegisterValidators(new[] { typeof(TestValidator).Assembly });
            container.Verify();

            var validators = container.GetAllInstances<IValidator<TestObject>>();
            Assert.IsType<DataAnnotationsValidator<TestObject>>(validators.First());
        }

        [Fact]
        public void RegisteresWithOverridingEnabled()
        {
            var container = new Container();
            container.Options.AllowOverridingRegistrations = true;
            container.RegisterValidators(new[] { typeof(TestValidator).Assembly });
            container.Verify();

            var validators = container.GetAllInstances<IValidator<TestObject>>();
            Assert.Equal(3, validators.Count());
        }
    }

    public class TestValidator : IValidator<TestObject>
    {
        public Task<ValidationResult> ValidateAsync(TestObject instance, CancellationToken cancellationToken)
            => Task.FromResult(ValidationResult.Success());
    }

    public class TestGenericValidator<T> : IValidator<T> where T : notnull
    {
        public Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken)
            => Task.FromResult(ValidationResult.Success());
    }
}