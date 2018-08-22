using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LightInject.Microsoft.DependencyInjection.Tests
{
    public class ServiceProviderFactoryTests
    {
        [Fact]
        public void ShouldCreateBuilder()
        {
            var serviceCollection = new ServiceCollection();
            var factory = new LightInjectServiceProviderFactory();
            var container = factory.CreateBuilder(serviceCollection);
            
        }

        [Fact]
        public void ShouldCreateServiceProvider()
        {
            var serviceCollection = new ServiceCollection();
            var factory = new LightInjectServiceProviderFactory();
            var container = factory.CreateBuilder(serviceCollection);
            var provider = factory.CreateServiceProvider(container);

            Assert.IsAssignableFrom<IServiceContainer>(container);
        }

        [Fact]
        public void ShouldDisposePerContainerServicesWhenProviderIsDisposed()
        {
            var serviceCollection = new ServiceCollection();            
            var factory = new LightInjectServiceProviderFactory();
            var container = factory.CreateBuilder(serviceCollection);
            container.Register<DisposableFoo>(new PerContainerLifetime());
            using (var provider = (IDisposable)container.CreateServiceProvider(serviceCollection))
            {
                ((IServiceProvider)provider).GetService<DisposableFoo>();
            }

            Assert.True(DisposableFoo.IsDisposed);
        }

        public class DisposableFoo : IDisposable
        {
            public static bool IsDisposed;

            public void Dispose()
            {
                IsDisposed = true;
            }
        }
    }
}
