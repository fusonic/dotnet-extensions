// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Fusonic.Extensions.AspNetCore.Tests;

public class TestWebHostEnvironment : IWebHostEnvironment
{
    public TestWebHostEnvironment()
    {
        var currentDir = Environment.CurrentDirectory;
        var fileProvider = new PhysicalFileProvider(currentDir);

        ContentRootFileProvider = WebRootFileProvider = fileProvider;
        ContentRootPath = WebRootPath = currentDir;

        ApplicationName = Assembly.GetExecutingAssembly().GetName().Name!;
    }

    public string ApplicationName { get; set; }
    public string EnvironmentName { get; set; } = "Test";

    public IFileProvider ContentRootFileProvider { get; set; }
    public IFileProvider WebRootFileProvider { get; set; }

    public string ContentRootPath { get; set; }
    public string WebRootPath { get; set; }
}