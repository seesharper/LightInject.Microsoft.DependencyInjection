using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightInject.Microsoft.DependencyInjection.Tests
{
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.DependencyInjection.Specification;
    using global::Microsoft.Extensions.DependencyInjection.Specification.Fakes;
    using Xunit;

    public class LightInjectSpecificationTests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {         
            var container = new ServiceContainer();            
            return container.CreateServiceProvider(serviceCollection);            
        }                
    }
}
