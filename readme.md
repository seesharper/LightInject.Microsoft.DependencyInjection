# LightInject.Microsoft.DependencyInjection

Enables **LightInject** to be used as the service container in ASP.NET Core and Entity Framework 7 applications.

> Note: This a release candidate. 




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


