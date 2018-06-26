namespace LightInject.Microsoft.DependencyInjection.Tests
{
    using System;
    using System.Linq;
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.DependencyInjection.Specification;

    public class LightInjectSpecificationTests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {                        
            var container = new ServiceContainer(new ContainerOptions() { EnablePropertyInjection = false, DefaultServiceSelector = services => services.Last() });
            return container.CreateServiceProvider(serviceCollection);            
        }                
    }
}
