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


        [Fact]
        public void SingletonServiceCanBeResolvedFromScope()
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.AddSingleton<ClassWithServiceProvider>();
            var provider = collection.CreateLightInjectServiceProvider();

            // Act
            IServiceProvider scopedSp1 = null;
            IServiceProvider scopedSp2 = null;
            ClassWithServiceProvider instance1 = null;
            ClassWithServiceProvider instance2 = null;

            using (var scope1 = provider.CreateScope())
            {
                scopedSp1 = scope1.ServiceProvider;
                instance1 = scope1.ServiceProvider.GetRequiredService<ClassWithServiceProvider>();
            }

            using (var scope2 = provider.CreateScope())
            {
                scopedSp2 = scope2.ServiceProvider;
                instance2 = scope2.ServiceProvider.GetRequiredService<ClassWithServiceProvider>();
            }

            // Assert
            Assert.Same(instance1.ServiceProvider, instance2.ServiceProvider);
            Assert.NotSame(instance1.ServiceProvider, scopedSp1);
            Assert.NotSame(instance2.ServiceProvider, scopedSp2);
        }


        [Fact]
        public void ShouldHandleDefaultArgument()
        {
            var collection = new ServiceCollection();
            collection.AddTransient<ClassWithDefaultStringArgument>();
            var provider = collection.CreateLightInjectServiceProvider();

            var instance = provider.GetService<ClassWithDefaultStringArgument>();
            Assert.Equal("42", instance.Value);
        }

        public class Foo
        {
        }

        public class DerivedFoo : Foo
        {
        }

        public class ClassWithServiceProvider
        {
            public ClassWithServiceProvider(IServiceProvider serviceProvider)
            {
                ServiceProvider = serviceProvider;
            }

            public IServiceProvider ServiceProvider { get; }
        }

        public class ClassWithDefaultStringArgument
        {
            public ClassWithDefaultStringArgument(string value = "42")
            {
                Value = value;
            }

            public string Value { get; }
        }
    }
}