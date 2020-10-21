using System;
using System.Net.Http;
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

        [Fact]
        public void ShouldHandleCorrectScopeWhenInjectedAlongWithHttpClient()
        {
            var collection = new ServiceCollection();
            collection.AddScoped<IScoped>(sp => new Scoped());
            collection.AddHttpClient<MyClient>();
            var provider = collection.CreateLightInjectServiceProvider();
            IScoped firstScoped;
            IScoped secondScoped;
            using (var scope1 = provider.CreateScope())
            {
                var instance = scope1.ServiceProvider.GetService<MyClient>();
                firstScoped = instance.Scoped;
            }

            using (var scope2 = provider.CreateScope())
            {
                var instance = scope2.ServiceProvider.GetService<MyClient>();
                secondScoped = instance.Scoped;
            }

            Assert.NotSame(firstScoped, secondScoped);
        }

        [Fact]
        public void ShouldNotDisposeScopedServiceWhenInjectedIntoSingletonUsingDefaultProvider()
        {
            var collection = new ServiceCollection();
            collection.AddSingleton<SingletonWithScopedDisposable>();
            collection.AddScoped<ScopedDisposable>();

            var provider = collection.BuildServiceProvider();

            SingletonWithScopedDisposable singletonDisposable;
            using (var scope = provider.CreateScope())
            {
                singletonDisposable = scope.ServiceProvider.GetService<SingletonWithScopedDisposable>();
            }

            Assert.False(singletonDisposable.Disposable.IsDisposed);
        }

        [Fact]
        public void ShouldNotDisposeScopedServiceWhenInjectedIntoSingletonUsingLightInjectProvider()
        {
            var collection = new ServiceCollection();
            collection.AddSingleton<SingletonWithScopedDisposable>();
            collection.AddScoped<ScopedDisposable>();

            var provider = collection.CreateLightInjectServiceProvider();

            SingletonWithScopedDisposable singletonDisposable;
            using (var scope = provider.CreateScope())
            {
                singletonDisposable = scope.ServiceProvider.GetService<SingletonWithScopedDisposable>();
            }

            Assert.False(singletonDisposable.Disposable.IsDisposed);
        }

        public class SingletonWithScopedDisposable
        {
            public SingletonWithScopedDisposable(ScopedDisposable disposable)
            {
                Disposable = disposable;
            }

            public ScopedDisposable Disposable { get; }

        }

        public class ScopedDisposable : IDisposable
        {
            public bool IsDisposed;

            public ScopedDisposable()
            {
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        public class MyClient
        {
            public MyClient(HttpClient httpClient, IScoped scoped)
            {
                Scoped = scoped;
            }

            public IScoped Scoped { get; }
        }


        public interface IFoo
        {

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



        public interface IScoped
        {

        }

        public class Scoped : IScoped
        {
            public static int InstanceCount;

            public Scoped()
            {
                InstanceCount++;
            }
        }
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
