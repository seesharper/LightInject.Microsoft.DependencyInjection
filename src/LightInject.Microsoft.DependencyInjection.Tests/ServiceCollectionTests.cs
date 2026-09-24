using System;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        [Fact]
        public void ShouldUseImplementationTypeForKeyedOpenGenericDescriptorWithFactory()
        {
            // Mimics NServiceBus' KeyedServiceCollectionAdapter that sets both a factory and the implementation type.
            var descriptor = new ServiceDescriptor(typeof(IThing<>), "MyKey", (sp, key) => throw new InvalidOperationException(), ServiceLifetime.Singleton);
            GetImplementationType(descriptor) = typeof(Thing<>);
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.Add(descriptor);

            var provider = serviceCollection.CreateLightInjectServiceProvider();

            Assert.IsType<Thing<int>>(provider.GetRequiredKeyedService<IThing<int>>("MyKey"));
        }

        [Fact]
        public void ShouldUseImplementationTypeForOpenGenericDescriptorWithFactory()
        {
            var descriptor = new ServiceDescriptor(typeof(IThing<>), sp => throw new InvalidOperationException(), ServiceLifetime.Singleton);
            GetImplementationType(descriptor) = typeof(Thing<>);
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.Add(descriptor);

            var provider = serviceCollection.CreateLightInjectServiceProvider();

            Assert.IsType<Thing<int>>(provider.GetRequiredService<IThing<int>>());
        }

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_implementationType")]
        private static extern ref Type GetImplementationType(ServiceDescriptor descriptor);

        public interface IThing<T>
        {
        }

        public class Thing<T> : IThing<T>
        {
        }
    }
}
