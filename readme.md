# LightInject.Microsoft.DependencyInjection
[![AppVeyor](https://img.shields.io/appveyor/ci/gruntjs/grunt.svg?maxAge=2592000)](https://ci.appveyor.com/project/seesharper/lightinject-microsoft-dependencyinjection)
[![NuGet](https://img.shields.io/nuget/v/LightInject.Microsoft.DependencyInjection.svg?maxAge=2592000)]()
[![GitHub tag](https://img.shields.io/github/tag/seesharper/LightInject.Microsoft.DependencyInjection.svg?maxAge=2592000)]()

Implements the [Microsoft.Extensions.DependencyInjection.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection.Abstractions/) and makes it possible to create an `IServiceProvider` that is 100% compatible with the [Microsoft.Extensions.DependencyInjection.Specification.Tests](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection.Specification.Tests). 

> Note: This package is NOT meant to be used directly with AspNetCore applications. If the target application is an AspNetCore application, use the [LightInject.Microsoft.Hosting](https://www.nuget.org/packages/LightInject.Microsoft.Hosting/) package instead. 

## Installing

```shell
dotnet add package LightInject.Microsoft.DependencyInjection
```

## Usage
```c#
var services = new ServiceCollection();
services.AddTransient<Foo>();
var provider = services.CreateLightInjectServiceProvider();
```

It is also possible to create an `IServiceProvider` directly from an `IServiceContainer` instance.

```c#
var container = new ServiceContainer(Options.Default.WithMicrosoftSettings);
var provider = container.CreateServiceProvider();
```

> Note: Make sure that the `Options.Default.WithMicrosoftSettings` is passed in as `options` when creating the container. This makes the provider compliant with the default provider from Microsoft. 

