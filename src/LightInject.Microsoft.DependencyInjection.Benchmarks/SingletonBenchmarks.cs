using System;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace LightInject.Microsoft.DependencyInjection.Benchmarks
{
    public class SingletonBenchmarks
    {
        private IServiceProvider lightInjectServiceProvider;
        private IServiceProvider defaultServiceProvider;

        [GlobalSetup]
        public void Setup()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ISingleton1, Singleton1>();

            lightInjectServiceProvider = serviceCollection.CreateLightInjectServiceProvider();
            defaultServiceProvider = serviceCollection.BuildServiceProvider();
        }

        [Benchmark]
        public void UsingLightInject()
        {
            var instance = lightInjectServiceProvider.GetService<ISingleton1>();
        }

        [Benchmark]
        public void UsingMicrosoft()
        {
            var instance = defaultServiceProvider.GetService<ISingleton1>();
        }

    }
}