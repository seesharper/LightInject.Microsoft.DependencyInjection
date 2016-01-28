# LightInject.Microsoft.DependencyInjection

[![Build status](https://ci.appveyor.com/api/projects/status/opvt2on49ta4i8v4?svg=true)](https://ci.appveyor.com/project/seesharper/lightinject-microsoft-dependencyinjection)

Enables **LightInject** to be used as the service container in ASP.NET Core and Entity Framework 7 applications.

> Note: This package is currently in pre release and you need to add https://www.myget.org/F/aspnetvnext/api/v3/index.json to your package sources. 
 

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
}

```


