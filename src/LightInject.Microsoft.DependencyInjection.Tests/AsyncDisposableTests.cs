using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LightInject.Microsoft.DependencyInjection.Tests
{
    public class AsyncDisposableTests
    {
        [Fact]
        public async Task ShouldDisposeAsyncDisposable()
        {
            var serviceCollection = new ServiceCollection();
            List<object> disposedObjects = new();
            serviceCollection.AddScoped<AsyncDisposable>(sp => new AsyncDisposable(disposedObject => disposedObjects.Add(disposedObject)));

            var serviceProvider = serviceCollection.CreateLightInjectServiceProvider();

            AsyncDisposable asyncDisposable = null;
            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                asyncDisposable = scope.ServiceProvider.GetService<AsyncDisposable>();
            }

            Assert.Contains(asyncDisposable, disposedObjects);
        }

        [Fact]
        public async Task ShouldDisposeAsyncDisposableFromRootScope()
        {
            var serviceCollection = new ServiceCollection();
            List<object> disposedObjects = new();
            serviceCollection.AddSingleton<AsyncDisposable>(sp => new AsyncDisposable(disposedObject => disposedObjects.Add(disposedObject)));

            var serviceProvider = serviceCollection.CreateLightInjectServiceProvider();

            AsyncDisposable asyncDisposable = null;

            asyncDisposable = serviceProvider.GetService<AsyncDisposable>();
            await ((IAsyncDisposable)serviceProvider).DisposeAsync();

            // Call it twice to ensure only disposed once. 
            await ((IAsyncDisposable)serviceProvider).DisposeAsync();


            Assert.Contains(asyncDisposable, disposedObjects);
            Assert.Single(disposedObjects);
        }

        [Fact]
        public async Task ShouldHandleDisposeAsyncOnServiceProvider()
        {

        }
    }

    public class AsyncDisposable : IAsyncDisposable
    {
        private readonly Action<object> onDisposed;

        public AsyncDisposable(Action<object> onDisposed)
        {
            this.onDisposed = onDisposed;
        }
        public ValueTask DisposeAsync()
        {
            onDisposed(this);
            return ValueTask.CompletedTask;
        }
    }

    public class Disposable : IDisposable
    {
        private readonly Action<object> onDisposed;

        public Disposable(Action<object> onDisposed)
        {
            this.onDisposed = onDisposed;
        }

        public void Dispose()
        {
            onDisposed(this);
        }
    }
}