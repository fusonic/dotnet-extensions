// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using FluentAssertions;
using Fusonic.Extensions.Common.Reflection;
using Xunit;

namespace Fusonic.Extensions.Common.Tests.Reflection;

public class PropertyUtilTests
{
    [Fact]
    public void GetName_Generic_Unary_ReturnsPropertyName()
        => PropertyUtil.GetName<TestClass>(x => x.IsBusy).Should().Be(nameof(TestClass.IsBusy));

    [Fact]
    public void GetName_Generic_Member_ReturnsPropertyName()
        => PropertyUtil.GetName<TestClass>(x => x.Value).Should().Be(nameof(TestClass.Value));

    [Fact]
    public void GetName_Generic_InvalidExpression_ThrowsException()
    {
        Action action = () => PropertyUtil.GetName<TestClass>(x => x.GetType());
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetName_GenericExplicit_Unary_ReturnsPropertyName()
        => PropertyUtil.GetName<TestClass, bool>(x => x.IsBusy).Should().Be(nameof(TestClass.IsBusy));

    [Fact]
    public void GetName_GenericExplicit_Member_ReturnsPropertyName()
        => PropertyUtil.GetName<TestClass, string>(x => x.Value).Should().Be(nameof(TestClass.Value));

    [Fact]
    public void GetName_GenericExplicit_InvalidExpression_ThrowsException()
    {
        Action action = () => PropertyUtil.GetName<TestClass, Type>(x => x.GetType());
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetType_Unary_ReturnsType()
        => PropertyUtil.GetType<TestClass>(x => x.IsBusy).Should().Be(typeof(bool));

    [Fact]
    public void GetType_Member_ReturnsType()
        => PropertyUtil.GetType<TestClass>(x => x.Value).Should().Be(typeof(string));

    [Fact]
    public void GetType_InvalidExpression_ThrowsException()
    {
        Action action = () => PropertyUtil.GetType<TestClass>(x => x.GetType());
        action.Should().Throw<ArgumentException>();
    }

    public class TestClass
    {
        public bool IsBusy { get; set; }
        public required string Value { get; set; }
    }
}
