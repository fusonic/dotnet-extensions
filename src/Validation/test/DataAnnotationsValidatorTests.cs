// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Collections;
using FluentAssertions;
using Xunit;

namespace Fusonic.Extensions.Validation.Tests;

public class DataAnnotationsValidatorTests
{
    [Fact]
    public void ValidatesObject()
    {
        var result = DataAnnotationsValidator.Validate(new TestObject());
        Assert.False(result.IsValid);
        Assert.Collection(result.Errors,
            e =>
            {
                Assert.Equal("Test", e.Key);
                Assert.Collection(e.Value, er => Assert.Equal("The Test field is required.", er));
            },
            e =>
            {
                Assert.Equal("Test2", e.Key);
                Assert.Collection(e.Value, er => Assert.Equal("The field Test2 must be between 5 and 10.", er));
            });
    }

    [Fact]
    public void ValidatesIValidateableObject()
    {
        var result = DataAnnotationsValidator.Validate(new TestValidatableObject());
        Assert.False(result.IsValid);
        Assert.Collection(result.Errors,
            e =>
            {
                Assert.Equal("", e.Key);
                Assert.Collection(e.Value, er => Assert.Equal("Error without member", er));
            },
            e =>
            {
                Assert.Equal("Test", e.Key);
                Assert.Collection(e.Value,
                    er => Assert.Equal("Error with member", er),
                    er => Assert.Equal("Error with multiple members", er));
            },
            e =>
            {
                Assert.Equal("Test2", e.Key);
                Assert.Collection(e.Value, er => Assert.Equal("Error with multiple members", er));
            });
    }

    [Fact]
    public void ValidatesChildObjects()
    {
        var model = new TestObjectWithChild()
        {
            Test = new TestItem()
        };

        var result = DataAnnotationsValidator.Validate(model);
        Assert.False(result.IsValid);
        Assert.Collection(result.Errors,
            e =>
            {
                Assert.Equal("Test.Test", e.Key);
                Assert.Collection(e.Value, er => Assert.Equal("The Test field is required.", er));
            });
    }

    [Fact]
    public void ValidatesCollections()
    {
        var model = new TestObjectWithList()
        {
            Test = new List<TestItem>()
                {
                    new TestItem(),
                    new TestItem()
                    {
                        Test = "valid"
                    },
                    new TestItem()
                }
        };

        var result = DataAnnotationsValidator.Validate(model);
        Assert.False(result.IsValid);
        Assert.Collection(result.Errors,
            e =>
            {
                Assert.Equal("Test[0].Test", e.Key);
                Assert.Collection(e.Value, er => Assert.Equal("The Test field is required.", er));
            },
            e =>
            {
                Assert.Equal("Test[2].Test", e.Key);
                Assert.Collection(e.Value, er => Assert.Equal("The Test field is required.", er));
            });
    }

    [Fact]
    public void ValidatesNullCollections()
    {
        var model = new TestObjectWithList
        {
            Test = null
        };

        var result = DataAnnotationsValidator.Validate(model);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CircularReference_NoStackOverflow()
    {
        var model = new SelfReferencingTestObject
        {
            Test = new SelfReferencingTestObject()
        };
        model.Test.Test = model;

        var result = DataAnnotationsValidator.Validate(model);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    // Header dictionary implements IEnumerable but not ICollection
    public void ValidatesHeaderDictionary()
    {
        var dictionary = new
        {
            Dict = new HeaderDictionary()
        };

        var result = DataAnnotationsValidator.Validate(dictionary);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ReturnsTrueIfValidatingPrimitive()
    {
        var result = DataAnnotationsValidator.Validate("test");
        Assert.True(result.IsValid);
        result = DataAnnotationsValidator.Validate(2134);
        Assert.True(result.IsValid);
    }

    private class HeaderDictionary : IEnumerable
    {
        public IEnumerator GetEnumerator() { yield break; }
    }
}
