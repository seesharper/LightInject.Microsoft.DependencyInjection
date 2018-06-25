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
            Func<string[], string> defaultServiceNameSelector = services =>
            {
                if (services.Length == 0)
                {
                    return string.Empty;
                }
                else
                {
                    return services.OrderBy(s => s).Last();
                }
            };

            var container = new ServiceContainer(new ContainerOptions() { EnablePropertyInjection = false, DefaultServiceSelector = defaultServiceNameSelector });
            return container.CreateServiceProvider(serviceCollection);            
        }                
    }
}
