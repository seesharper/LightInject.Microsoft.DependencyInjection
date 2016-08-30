# LightInject.Microsoft.DependencyInjection
[![AppVeyor](https://img.shields.io/appveyor/ci/gruntjs/grunt.svg?maxAge=2592000)](https://ci.appveyor.com/project/seesharper/lightinject-microsoft-dependencyinjection)
[![NuGet](https://img.shields.io/nuget/v/LightInject.Microsoft.DependencyInjection.svg?maxAge=2592000)]()
[![GitHub tag](https://img.shields.io/github/tag/seesharper/LightInject.Microsoft.DependencyInjection.svg?maxAge=2592000)]()

[Changelog](https://github.com/seesharper/LightInject.Microsoft.DependencyInjection/blob/master/CHANGELOG.md)

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


