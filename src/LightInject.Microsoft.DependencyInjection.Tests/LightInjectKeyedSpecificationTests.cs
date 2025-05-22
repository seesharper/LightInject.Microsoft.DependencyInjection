using System;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;
using Xunit;
using Xunit.Sdk;

namespace LightInject.Microsoft.DependencyInjection.Tests;

public class LightInjectKeyedSpecificationTests : KeyedDependencyInjectionSpecificationTests
{
    protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
    {
        return serviceCollection.CreateLightInjectServiceProvider(new ContainerOptions() { EnableCurrentScope = false });
    }

    [Fact]
    public void ShouldHandleKeyedServiceWithEnumKey()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<IKeyedService, KeyedService>(Key.A);
        var provider = CreateServiceProvider(serviceCollection);
        provider.GetKeyedService<IKeyedService>(Key.A);
    }

    [Fact]
    public void ShouldHandleKeyedServiceWithStringServiceKey()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<IKeyedService, KeyedServiceWithStringServiceKey>("A");
        var provider = CreateServiceProvider(serviceCollection);
        var instance = provider.GetKeyedService<IKeyedService>("A");
        Assert.Equal("A", ((KeyedServiceWithStringServiceKey)instance).ServiceKey);
    }

    [Fact]
    public void ShouldHandleRequiredKeyedServiceWithStringServiceKeyUsingFactory()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<IKeyedService>(Key.A, (sp, key) => new KeyedServiceWithStringServiceKey((string)key));
        var provider = CreateServiceProvider(serviceCollection);
        var instance = provider.GetRequiredKeyedService<IKeyedService>("A");
        Assert.Equal("A", ((KeyedServiceWithStringServiceKey)instance).ServiceKey);
    }

    [Fact]
    public void ShouldHandleKeyedServiceWithEnumServiceKey()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<IKeyedService, KeyedServiceWithEnumServiceKey>(Key.A);
        var provider = CreateServiceProvider(serviceCollection);
        var instance = provider.GetKeyedService<IKeyedService>(Key.A);
        Assert.Equal(Key.A, ((KeyedServiceWithEnumServiceKey)instance).ServiceKey);
    }

    [Fact]
    public void ShouldHandleKeyedServiceWithIntServiceKey()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<IKeyedService, KeyServiceWithIntKey>(1);
        var provider = CreateServiceProvider(serviceCollection);
        var instance = provider.GetKeyedService<IKeyedService>(1);
        Assert.Equal(1, ((KeyServiceWithIntKey)instance).ServiceKey);
    }

    [Fact]
    public void ShouldHandlePassingServiceKeyAsInteger()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<IKeyedService>(1, (sp, key) => new KeyServiceWithIntKey((int)key));
        var provider = CreateServiceProvider(serviceCollection);
        var instance = provider.GetKeyedService<IKeyedService>(1);
        Assert.Equal(1, ((KeyServiceWithIntKey)instance).ServiceKey);
    }

    [Fact]
    public void ShouldHandlePassingServiceKeyAsEnum()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<IKeyedService>(Key.A, (sp, key) => new KeyedServiceWithEnumServiceKey((Key)key));
        var provider = CreateServiceProvider(serviceCollection);
        var instance = provider.GetKeyedService<IKeyedService>(Key.A);
        Assert.Equal(Key.A, ((KeyedServiceWithEnumServiceKey)instance).ServiceKey);
    }

    [Fact]
    public void ShouldThrowExceptionWhenUsingInvalidKeyType()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<IKeyedService>(new StringBuilder(), (sp, key) => new KeyedServiceWithEnumServiceKey((Key)key));
        var provider = CreateServiceProvider(serviceCollection);
        Assert.Throws<InvalidOperationException>(() => provider.GetKeyedService<IKeyedService>(new StringBuilder()));
    }

    [Fact]
    public void ShouldHandleConvertibleType()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<IKeyedService>(1L, (sp, key) => new KeyedServiceWithLongServiceKey((long)key));
        var provider = CreateServiceProvider(serviceCollection);
        var instance = provider.GetKeyedService<IKeyedService>(1L);
        Assert.Equal(1L, ((KeyedServiceWithLongServiceKey)instance).ServiceKey);
    }

    [Fact]
    public void ShouldHandlePassingNullAsServiceKeyForRequiredService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<IKeyedService, KeyedService>(Key.A);
        var provider = CreateServiceProvider(serviceCollection);
        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredKeyedService<IKeyedService>(null));
    }

    [Fact]
    public void ShouldInjectKeyedService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<IKeyedService, KeyedService>("KeyedService");
        serviceCollection.AddKeyedTransient<IKeyedService, AnotherKeyedService>("AnotherKeyedService");
        serviceCollection.AddTransient<ServiceWithKeyedService>();
        var provider = CreateServiceProvider(serviceCollection);
        var instance = provider.GetService<ServiceWithKeyedService>();
        Assert.IsType<AnotherKeyedService>(instance.Service);
    }

    [Fact]
    public void ShouldInjectServiceFromDerivedServiceKey()
    {
         var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<IKeyedService, KeyedService>("KeyedService");
        serviceCollection.AddKeyedTransient<IKeyedService, AnotherKeyedService>("AnotherKeyedService");
        serviceCollection.AddTransient<ServiceWithDerivedServiceKey>();
        var provider = CreateServiceProvider(serviceCollection);
        var instance = provider.GetService<ServiceWithDerivedServiceKey>();
        Assert.IsType<AnotherKeyedService>(instance.Service);
    }
}

public interface IKeyedService
{
}

public class KeyedService : IKeyedService
{

}

public class KeyedServiceWithStringServiceKey([ServiceKey] string serviceKey) : IKeyedService
{
    public string ServiceKey { get; } = serviceKey;
}

public class KeyedServiceWithEnumServiceKey([ServiceKey] Key serviceKey) : IKeyedService
{
    public Key ServiceKey { get; } = serviceKey;
}


public class KeyServiceWithIntKey([ServiceKey] int serviceKey) : IKeyedService
{
    public int ServiceKey { get; } = serviceKey;
}


public class KeyedServiceWithLongServiceKey([ServiceKey] long serviceKey) : IKeyedService
{
    public long ServiceKey { get; } = serviceKey;
}

public enum Key
{
    A,
    B
}

public class AnotherKeyedService : IKeyedService
{
}


public class ServiceWithKeyedService([FromKeyedServices("AnotherKeyedService")] IKeyedService service)
{
    public IKeyedService Service { get; } = service;
}


public class DerivedServiceKey : FromKeyedServicesAttribute
{
    public DerivedServiceKey() : base("AnotherKeyedService")
    {
    }
}


public class ServiceWithDerivedServiceKey([DerivedServiceKey] IKeyedService service)
{
    public IKeyedService Service { get; } = service;
}