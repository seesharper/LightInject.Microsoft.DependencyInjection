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
            Func<string[], string> defaultServiceNameSelector = services =>
            {
                if (services.Length == 0)
                {
                    return string.Empty;
                }
                else
                {
                    return services.OrderBy(s => s).Last();
                }
            };


            var container = new ServiceContainer(new ContainerOptions() { EnablePropertyInjection = false, DefaultServiceSelector = defaultServiceNameSelector });
            return container.CreateServiceProvider(serviceCollection);
            //return serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateScopes = false });
        }   
        
        [Fact]
        public void Test()
        {
            var serviceCollection = new ServiceCollection();
            var instance = new FakeOpenGenericService<PocoClass>(null);

            serviceCollection.AddTransient<PocoClass, PocoClass>();
            serviceCollection.AddSingleton(typeof(IFakeOpenGenericService<PocoClass>), typeof(FakeService));
            serviceCollection.AddSingleton(typeof(IFakeOpenGenericService<>), typeof(FakeOpenGenericService<>));
            serviceCollection.AddSingleton<IFakeOpenGenericService<PocoClass>>(instance);

            var serviceProvider = CreateServiceProvider(serviceCollection);

            var enumerable = serviceProvider.GetService<IEnumerable<IFakeOpenGenericService<PocoClass>>>().ToArray();

            // Assert
            Assert.Equal(3, enumerable.Length);
            Assert.NotNull(enumerable[0]);
            Assert.NotNull(enumerable[1]);
            Assert.NotNull(enumerable[2]);

            Assert.Equal(instance, enumerable[2]);
            Assert.IsType<FakeService>(enumerable[0]);
        }

        [Fact]
        public void Test2()
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.AddSingleton<IFakeSingletonService, FakeService>();
            collection.AddScoped<IFakeScopedService, FakeService>();
            collection.AddTransient<IFakeService, FakeService>();

            var provider = CreateServiceProvider(collection);
            FakeService disposableService;
            FakeService transient1;
            FakeService transient2; 
            FakeService singleton;

            // Act and Assert
            var transient3 = Assert.IsType<FakeService>(provider.GetService<IFakeService>());
            using (var scope = provider.CreateScope())
            {
                disposableService = (FakeService)scope.ServiceProvider.GetService<IFakeScopedService>();
                transient1 = (FakeService)scope.ServiceProvider.GetService<IFakeService>();
                transient2 = (FakeService)scope.ServiceProvider.GetService<IFakeService>();
                singleton = (FakeService)scope.ServiceProvider.GetService<IFakeSingletonService>();

                Assert.False(disposableService.Disposed);
                Assert.False(transient1.Disposed);
                Assert.False(transient2.Disposed);
                Assert.False(singleton.Disposed);
            }

            Assert.True(disposableService.Disposed);
            Assert.True(transient1.Disposed);
            Assert.True(transient2.Disposed);
            Assert.False(singleton.Disposed);

            var disposableProvider = provider as IDisposable;
            if (disposableProvider != null)
            {
                disposableProvider.Dispose();
                Assert.True(singleton.Disposed);
                Assert.True(transient3.Disposed);
            }
        }

        [Fact]
        public void LightInjectDisposesInReverseOrderOfCreation()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<FakeDisposeCallback>();
            serviceCollection.AddTransient<IFakeOuterService, FakeDisposableCallbackOuterService>();
            serviceCollection.AddSingleton<IFakeMultipleService, FakeDisposableCallbackInnerService>();
            serviceCollection.AddSingleton<IFakeMultipleService, FakeDisposableCallbackInnerService>();
            serviceCollection.AddTransient<IFakeMultipleService, FakeDisposableCallbackInnerService>();
            serviceCollection.AddSingleton<IFakeService, FakeDisposableCallbackInnerService>();
            var serviceProvider = CreateServiceProvider(serviceCollection);

            var callback = serviceProvider.GetService<FakeDisposeCallback>();
            var outer = serviceProvider.GetService<IFakeOuterService>();
            var multipleServices = outer.MultipleServices.ToArray();

            // Act
            ((IDisposable)serviceProvider).Dispose();

            // Assert
            Assert.Equal(outer, callback.Disposed[0]);
            Assert.Equal(multipleServices.Reverse(), callback.Disposed.Skip(1).Take(3).OfType<IFakeMultipleService>());
            Assert.Equal(outer.SingleService, callback.Disposed[4]);
        }
    }
}
