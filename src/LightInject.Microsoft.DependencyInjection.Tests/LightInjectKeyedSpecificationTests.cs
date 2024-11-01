using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;
using Xunit;

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

   
}

public interface IKeyedService
{
}

public class KeyedService : IKeyedService
{

}

public class KeyedServiceWithStringServiceKey : IKeyedService
{
    public KeyedServiceWithStringServiceKey([ServiceKey] string serviceKey)
    {
        ServiceKey = serviceKey;
    }

    public string ServiceKey { get; }
}

public class KeyedServiceWithEnumServiceKey : IKeyedService
{
    public KeyedServiceWithEnumServiceKey([ServiceKey] Key serviceKey)
    {
        ServiceKey = serviceKey;
    }

    public Key ServiceKey { get; }
}


public class KeyServiceWithIntKey : IKeyedService
{
    public KeyServiceWithIntKey([ServiceKey] int serviceKey)
    {
        ServiceKey = serviceKey;
    }

    public int ServiceKey { get; }
}


public enum Key
{
    A,
    B
}