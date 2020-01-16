using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LightInject.Microsoft.DependencyInjection.Tests
{
    public class ServiceContainerTests
    {
        [Fact]
        public void ShouldThrowExceptionWhenProviderIsCreatedTwice()
        {
            var container = new ServiceContainer();
            var serviceCollection = new ServiceCollection();

            var provider = container.CreateServiceProvider(serviceCollection);

            Assert.Throws<InvalidOperationException>(() => container.CreateServiceProvider(serviceCollection));
        }

        [Fact]
        public void ShouldThrowExceptionWhenProviderIsCreatedTwiceAndCurrentScopeIsDisabled()
        {
            var container = new ServiceContainer(c => c.EnableCurrentScope = false);
            var serviceCollection = new ServiceCollection();

            var provider = container.CreateServiceProvider(serviceCollection);

            Assert.Throws<InvalidOperationException>(() => container.CreateServiceProvider(serviceCollection));
        }

        [Fact]
        public void ShouldCallConfigureActionWhenCreatingServiceProvider()
        {
            bool wasCalled = false;

            var serviceCollection = new ServiceCollection();
            serviceCollection.CreateLightInjectServiceProvider(o => wasCalled = true);

            Assert.True(wasCalled);
        }

        [Fact]
        public void ShouldNotUseRootScopeForFactoryDelegate()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<DerivedFoo>();

            serviceCollection.AddScoped<Foo>(sp =>
            {
                return sp.GetService<DerivedFoo>();
            });

            var provider = serviceCollection.CreateLightInjectServiceProvider(options => options.EnableCurrentScope = false);

            Foo firstFoo;
            Foo secondFoo;

            using (var serviceScope = provider.CreateScope())
            {
                firstFoo = serviceScope.ServiceProvider.GetService<Foo>();
            }

            using (var serviceScope = provider.CreateScope())
            {
                secondFoo = serviceScope.ServiceProvider.GetService<Foo>();
            }

            Assert.NotSame(firstFoo, secondFoo);
        }

        public class Foo
        {
        }

        public class DerivedFoo : Foo
        {
        }
    }
}