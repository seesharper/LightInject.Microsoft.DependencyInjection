using System;
using System.Runtime.CompilerServices;
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
        serviceCollection.AddKeyedTransient<IKeyedServiceWithEnumServiceKey, KeyedServiceWithEnumServiceKey>(Key.A);
        var provider = CreateServiceProvider(serviceCollection);
        provider.GetKeyedService<IKeyedServiceWithEnumServiceKey>(Key.A);
    }

    [Fact]
    public void ShouldHandleKeyedServiceWithStringServiceKey()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<IKeyedServiceWithStringServiceKey, KeyedServiceWithStringServiceKey>("A");
        var provider = CreateServiceProvider(serviceCollection);
        var instance = provider.GetKeyedService<IKeyedServiceWithStringServiceKey>("A");
        Assert.Equal("A", ((KeyedServiceWithStringServiceKey)instance).ServiceKey);
    }

    [Fact]
    public void ShouldHandleRequiredKeyedServiceWithStringServiceKeyUsingFactory()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<IKeyedServiceWithStringServiceKey>(Key.A, (sp, key) => new KeyedServiceWithStringServiceKey((string)key));
        var provider = CreateServiceProvider(serviceCollection);
        var instance = provider.GetRequiredKeyedService<IKeyedServiceWithStringServiceKey>("A");
        Assert.Equal("A", ((KeyedServiceWithStringServiceKey)instance).ServiceKey);
    }

    [Fact]
    public void ShouldHandleKeyedServiceWithEnumServiceKey()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<IKeyedServiceWithEnumServiceKey, KeyedServiceWithEnumServiceKey>(Key.A);
        var provider = CreateServiceProvider(serviceCollection);
        var instance = provider.GetKeyedService<IKeyedServiceWithEnumServiceKey>(Key.A);
        Assert.Equal(Key.A, ((KeyedServiceWithEnumServiceKey)instance).ServiceKey);
    }

    [Fact]
    public void ShouldHandleKeyedServiceWithIntServiceKey()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<IKeyedServiceWithIntKey, KeyedServiceWithIntKey>(1);
        var provider = CreateServiceProvider(serviceCollection);
        var instance = provider.GetKeyedService<IKeyedServiceWithIntKey>(1);
        Assert.Equal(1, ((KeyedServiceWithIntKey)instance).ServiceKey);
    }

    [Fact]
    public void ShouldHandlePassingServiceKeyAsInteger()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<IKeyedServiceWithIntKey>(1, (sp, key) => new KeyedServiceWithIntKey((int)key));
        var provider = CreateServiceProvider(serviceCollection);
        var instance = provider.GetKeyedService<IKeyedServiceWithIntKey>(1);
        Assert.Equal(1, ((KeyedServiceWithIntKey)instance).ServiceKey);
    }

    [Fact]
    public void ShouldHandlePassingServiceKeyAsEnum()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<IKeyedServiceWithEnumServiceKey>(Key.A, (sp, key) => new KeyedServiceWithEnumServiceKey((Key)key));
        var provider = CreateServiceProvider(serviceCollection);
        var instance = provider.GetKeyedService<IKeyedServiceWithEnumServiceKey>(Key.A);
        Assert.Equal(Key.A, ((KeyedServiceWithEnumServiceKey)instance).ServiceKey);
    }

    [Fact]
    public void ShouldThrowExceptionWhenUsingInvalidKeyType()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<IKeyedServiceWithInvalidServiceKeyType>(new StringBuilder(), (sp, key) => new KeyedServiceWithInvalidServiceKeyType());
        var provider = CreateServiceProvider(serviceCollection);
        Assert.Throws<InvalidOperationException>(() => provider.GetKeyedService<IKeyedServiceWithInvalidServiceKeyType>(new StringBuilder()));
    }

    [Fact]
    public void ShouldHandleConvertibleType()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<IKeyedServiceWithLongServiceKey>(1L, (sp, key) => new KeyedServiceWithLongServiceKey((long)key));
        var provider = CreateServiceProvider(serviceCollection);
        var instance = provider.GetKeyedService<IKeyedServiceWithLongServiceKey>(1L);
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
        serviceCollection.AddKeyedScoped<IKeyedService>("KeyedService", (sp, key) => new KeyedService());
        serviceCollection.AddKeyedScoped<IKeyedService>("AnotherKeyedService", (sp, key) => new AnotherKeyedService());
        serviceCollection.AddScoped<IServiceWithKeyedService, ServiceWithKeyedService>();
        var provider = CreateServiceProvider(serviceCollection);
        using var scope = provider.CreateScope();
        var instance = scope.ServiceProvider.GetRequiredService<IServiceWithKeyedService>();
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
        var instance = provider.GetRequiredService<ServiceWithDerivedServiceKey>();
        Assert.IsType<AnotherKeyedService>(instance.Service);
    }
}


public interface IKeyedServiceWithInvalidServiceKeyType
{
}

public class KeyedServiceWithInvalidServiceKeyType() : IKeyedServiceWithInvalidServiceKeyType
{
    
}


public interface IKeyedService
{
}

public class KeyedService : IKeyedService
{

}

public interface IKeyedServiceWithStringServiceKey
{
    string ServiceKey { get; }
}

public class KeyedServiceWithStringServiceKey([ServiceKey] string serviceKey) : IKeyedServiceWithStringServiceKey
{
    public string ServiceKey { get; } = serviceKey;
}

public interface IKeyedServiceWithEnumServiceKey
{
    Key ServiceKey { get; }
}

public class KeyedServiceWithEnumServiceKey([ServiceKey] Key serviceKey) : IKeyedServiceWithEnumServiceKey
{
    public Key ServiceKey { get; } = serviceKey;
}

public interface IKeyedServiceWithIntKey
{
    int ServiceKey { get; }
}

public class KeyedServiceWithIntKey([ServiceKey] int serviceKey) : IKeyedServiceWithIntKey
{
    public int ServiceKey { get; } = serviceKey;
}

public interface IKeyedServiceWithLongServiceKey
{
    long ServiceKey { get; }
}

public class KeyedServiceWithLongServiceKey([ServiceKey] long serviceKey) : IKeyedServiceWithLongServiceKey
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

public interface IServiceWithKeyedService
{
    IKeyedService Service { get; }
}

public class ServiceWithKeyedService([FromKeyedServices("AnotherKeyedService")] IKeyedService service) : IServiceWithKeyedService
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