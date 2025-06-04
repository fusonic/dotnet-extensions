// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Hangfire.Tests;

public class MediatorHandlerContextTests
{
    [Theory]
    [InlineData(typeof(int), "Message: Int32 (System)")]
    [InlineData(typeof(List<int>), "Message: List<Int32> (System.Collections.Generic)")]
    [InlineData(typeof(List<List<int>>), "Message: List<List<Int32>> (System.Collections.Generic)")]
    [InlineData(typeof(List<List<List<int>>>), "Message: List<List<List<Int32>>> (System.Collections.Generic)")]
    public void DisplayName_ContainsMessageType_WithFriendlyGenericArgs(Type type, string expectedString)
    {
        var message = Activator.CreateInstance(type)!;
        var context = new MediatorHandlerContext(message, "HandlerType");

        context.DisplayName.Should().Be(expectedString);
    }

    [Fact]
    public void ToString_ReturnsDisplayName()
    {
        var context= new MediatorHandlerContext(new object(), "HandlerType");
        context.ToString().Should().Be("Message: Object (System)");

        context.DisplayName = "Hallo";
        context.ToString().Should().Be("Hallo");
    }
}