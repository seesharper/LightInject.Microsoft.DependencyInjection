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
    }
}