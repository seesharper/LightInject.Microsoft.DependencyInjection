namespace LightInject.Microsoft.DependencyInjection.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using global::Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class Issue14
    {
        [Fact(Skip ="")]
        public void ShouldCreateEnumerableRegistrationForGenericTypes()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IFoo<int>, Foo<int>>();
            serviceCollection.AddSingleton<IFoo<int>, AnotherFoo<int>>();
            var container = new ServiceContainer();
            container.CreateServiceProvider(serviceCollection);
            var enumerableRegistration = container.AvailableServices.SingleOrDefault(sr => sr.ServiceType == typeof(IEnumerable<IFoo<int>>));
            Assert.NotNull(enumerableRegistration);
        }        
    }

    public interface IFoo<T>
    {
    }

    public class Foo<T> : IFoo<T>
    {

    }

    public class AnotherFoo<T> : IFoo<int>
    {

    }
}