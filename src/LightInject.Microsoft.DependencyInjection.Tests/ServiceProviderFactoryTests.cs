using System;
using System.Collections.Generic;
using System.Linq;
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
        public void ShouldUseExistingContainer()
        {
            var serviceCollection = new ServiceCollection();
            var container = new ServiceContainer(ContainerOptions.Default.WithMicrosoftSettings());
            var factory = new LightInjectServiceProviderFactory(container);
            var builder = factory.CreateBuilder(serviceCollection);
            Assert.Same(container, builder);
        }

        [Fact]
        public void ShouldDisposePerContainerServicesWhenProviderIsDisposed()
        {
            var serviceCollection = new ServiceCollection();
            var factory = new LightInjectServiceProviderFactory();
            var container = factory.CreateBuilder(serviceCollection);
            container.Register<DisposableFoo>(new PerContainerLifetime());
            DisposableFoo foo;
            using (var provider = (IDisposable)container.CreateServiceProvider(serviceCollection))
            {
                foo = ((IServiceProvider)provider).GetService<DisposableFoo>();
            }

            Assert.True(foo.IsDisposed);
        }

        [Fact]
        public void ShouldDisposeRootScopeWhenProviderIsDisposed()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<DisposableFoo>();
            var factory = new LightInjectServiceProviderFactory();
            var container = factory.CreateBuilder(serviceCollection);
            var provider = factory.CreateServiceProvider(container);

            var foo = provider.GetService<DisposableFoo>();
            ((IDisposable)provider).Dispose();

            Assert.True(foo.IsDisposed);
        }

        [Fact]
        public void ShouldDisposeRootScopeWhenProviderRequestedFromContainerIsDisposed()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<DisposableFoo>();

            var factory = new LightInjectServiceProviderFactory(new ServiceContainer(ContainerOptions.Default.WithMicrosoftSettings()));
            var container = factory.CreateBuilder(serviceCollection);
            var provider = factory.CreateServiceProvider(container);
            var requestedProvider = provider.GetService<IServiceProvider>();

            var foo = provider.GetService<DisposableFoo>();
            ((IDisposable)requestedProvider).Dispose();

            Assert.True(foo.IsDisposed);
        }

        [Fact]
        public void ShouldCallConfigureAction()
        {
            bool wasCalled = false;
            var factory = new LightInjectServiceProviderFactory(o => wasCalled = true);
            Assert.True(wasCalled);
            var container = factory.CreateBuilder(new ServiceCollection());
            Assert.IsType<ServiceContainer>(container);
        }


        public class DisposableFoo : IDisposable
        {
            public bool IsDisposed;

            public void Dispose()
            {
                IsDisposed = true;
            }
        }
    }
}
