using System;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LightInject.Microsoft.DependencyInjection.Tests
{
    public class ServiceCollectionTests
    {
        [Fact]
        public void ShouldCreateServiceProviderFromServiceCollection()
        {
            var serviceCollection = new ServiceCollection();
            var provider = serviceCollection.CreateLightInjectServiceProvider();
            Assert.IsAssignableFrom<IServiceProvider>(provider);
        }

        [Fact]
        public void ShouldCreateServiceProviderWithOptionsFromServiceCollection()
        {
            StringBuilder log = new StringBuilder();
            ContainerOptions.Default.LogFactory = (t) => l => log.AppendLine(l.Message);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton("42");
            var provider = serviceCollection.CreateLightInjectServiceProvider();
            var instance = provider.GetService<string>();
            Assert.IsAssignableFrom<IServiceProvider>(provider);

            Assert.NotEmpty(log.ToString());
        }

        [Fact]
        public void ShouldSupportNonRuntimeTypeFactoryRegistrations()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(new TypeDelegator(typeof(int)), _ => 42);
            Assert.NotNull(serviceCollection.CreateLightInjectServiceProvider());
        }
    }
}
