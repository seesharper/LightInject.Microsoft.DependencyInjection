using System;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace LightInject.Microsoft.DependencyInjection.Benchmarks
{
    public class BeginScopeBenchmarks
    {
        private IServiceProvider lightInjectServiceProvider;
        private IServiceProvider defaultServiceProvider;

        [GlobalSetup]
        public void Setup()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<TestController1>();
            serviceCollection.AddTransient<TestController2>();
            serviceCollection.AddTransient<TestController3>();
            serviceCollection.AddTransient<IRepositoryTransient1, RepositoryTransient1>();
            serviceCollection.AddTransient<IRepositoryTransient2, RepositoryTransient2>();
            serviceCollection.AddTransient<IRepositoryTransient3, RepositoryTransient3>();
            serviceCollection.AddTransient<IRepositoryTransient4, RepositoryTransient4>();
            serviceCollection.AddTransient<IRepositoryTransient5, RepositoryTransient5>();
            serviceCollection.AddScoped<IScopedService1, ScopedService1>();
            serviceCollection.AddScoped<IScopedService2, ScopedService2>();
            serviceCollection.AddScoped<IScopedService3, ScopedService3>();
            serviceCollection.AddScoped<IScopedService4, ScopedService4>();
            serviceCollection.AddScoped<IScopedService5, ScopedService5>();
            serviceCollection.AddSingleton<ISingleton1, Singleton1>();
            serviceCollection.AddSingleton<ISingleton2, Singleton2>();
            serviceCollection.AddSingleton<ISingleton3, Singleton3>();
            lightInjectServiceProvider = serviceCollection.CreateLightInjectServiceProvider(new ContainerOptions() { EnableCurrentScope = false });
            defaultServiceProvider = serviceCollection.BuildServiceProvider();
        }

        [Benchmark]
        public void UsingLightInject()
        {
            MethodToBenchmark(lightInjectServiceProvider);
        }

        [Benchmark]
        public void UsingMicrosoft()
        {
            MethodToBenchmark(defaultServiceProvider);
        }

        private void MethodToBenchmark(IServiceProvider serviceProvider)
        {
            var factory = (IServiceScopeFactory)serviceProvider.GetService(typeof(IServiceScopeFactory));

            using (var scope = factory.CreateScope())
            {
                var controller = scope.ServiceProvider.GetService(typeof(TestController1));
            }

            factory = (IServiceScopeFactory)serviceProvider.GetService(typeof(IServiceScopeFactory));

            using (var scope = factory.CreateScope())
            {
                var controller = scope.ServiceProvider.GetService(typeof(TestController2));
            }

            factory = (IServiceScopeFactory)serviceProvider.GetService(typeof(IServiceScopeFactory));

            using (var scope = factory.CreateScope())
            {
                var controller = scope.ServiceProvider.GetService(typeof(TestController3));
            }
        }
    }
}