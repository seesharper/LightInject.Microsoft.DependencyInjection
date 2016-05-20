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

        //public void Test()
        //{
        //    // Arrange
        //    var collection = new ServiceCollection();
        //    collection.AddTransient(typeof(IFakeOpenGenericService<AnotherClass>), typeof(FakeService));
        //    collection.AddTransient(typeof(IFakeOpenGenericService<>), typeof(FakeOpenGenericService<>));
        //    collection.AddSingleton<AnotherClass>();
        //    var provider = CreateServiceProvider(collection);

        //    // Act
        //    var service = provider.GetService<IFakeOpenGenericService<AnotherClass>>();

        //    // Assert
        //    Assert.IsType<FakeService>(service);
        //}
    }
}
