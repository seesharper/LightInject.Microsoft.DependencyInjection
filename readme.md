# LightInject.Microsoft.DependencyInjection
[![AppVeyor](https://img.shields.io/appveyor/ci/gruntjs/grunt.svg?maxAge=2592000)](https://ci.appveyor.com/project/seesharper/lightinject-microsoft-dependencyinjection)
[![NuGet](https://img.shields.io/nuget/v/LightInject.Microsoft.DependencyInjection.svg?maxAge=2592000)]()
[![GitHub tag](https://img.shields.io/github/tag/seesharper/LightInject.Microsoft.DependencyInjection.svg?maxAge=2592000)]()

Enables **LightInject** to be used as the service container in ASP.NET Core and Entity Framework 7 applications.

## Installing
```
"dependencies": {
  "LightInject.Microsoft.DependencyInjection": "<version>"
}
```

## Usage
```
public class Startup
{       
    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        var container = new ServiceContainer();
        return container.CreateServiceProvider(services);
    }
    
    public void Configure(IApplicationBuilder app)
    {          
        app.Run(async (context) =>
        {
            await context.Response.WriteAsync("Hello from LightInject");
        });
    }
}
```

## Controllers

By default, controllers are not actually created by *LightInject*. They are created by the ASP.NET infrastructure and uses LightInject to resolve its dependencies. To enable LightInject to create the controller instances, we need to add the following line.

```csharp
services.AddMvc().AddControllersAsServices();
```



## .Net Core 2.0

**Requirements:**

* &gt;= LightInject 5.1.0
* &gt;= LightInject.Microsoft.DependencyInjection 2.0.3

In addition we need to turn of propertyinjection

```c#
var containerOptions = new ContainerOptions { EnablePropertyInjection = false } 
var container = new ServiceContainer(containerOptions);
```



