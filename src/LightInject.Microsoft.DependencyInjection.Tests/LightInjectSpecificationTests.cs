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
            return serviceCollection.CreateLightInjectServiceProvider();
        }
    }
}
