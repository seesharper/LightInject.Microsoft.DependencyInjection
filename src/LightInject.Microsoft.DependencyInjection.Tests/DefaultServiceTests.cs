using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LightInject.Microsoft.DependencyInjection.Tests;

public class DefaultServiceTests
{
    [Fact]
    public void ShouldOverrideDefaultRegistrationInServiceContainer()
    {
        var container = new ServiceContainer(options => options.WithMicrosoftSettings());
        container.RegisterTransient<IFoo, Foo>();
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IFoo, AnotherFoo>();

        var provider = container.CreateServiceProvider(serviceCollection);

        var foo = provider.GetRequiredService<IFoo>();

        Assert.IsType<AnotherFoo>(foo);
    }

    [Fact]
    public void ShouldOverrideNamedRegistrationInServiceContainer()
    {
        var container = new ServiceContainer(options => options.WithMicrosoftSettings());
        container.RegisterTransient<IFoo, Foo>("Foo");
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IFoo, AnotherFoo>();

        var provider = container.CreateServiceProvider(serviceCollection);

        var foo = provider.GetRequiredService<IFoo>();

        Assert.IsType<AnotherFoo>(foo);
    }

    [Fact]
    public void ShouldOverrideMultipleNamedRegistrationInServiceContainer()
    {
        var container = new ServiceContainer(options => options.WithMicrosoftSettings());
        container.RegisterTransient<IFoo, Foo>("Foo1");
        container.RegisterTransient<IFoo, Foo>("Foo2");
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IFoo, AnotherFoo>();

        var provider = container.CreateServiceProvider(serviceCollection);

        var foo = provider.GetRequiredService<IFoo>();

        Assert.IsType<AnotherFoo>(foo);
    }

    [Fact]
    public void ShouldOverrideNamedAndDefaultRegistrationInServiceContainer()
    {
        var container = new ServiceContainer(options => options.WithMicrosoftSettings());
        container.RegisterTransient<IFoo, Foo>();
        container.RegisterTransient<IFoo, Foo>("Foo");
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IFoo, AnotherFoo>();

        var provider = container.CreateServiceProvider(serviceCollection);

        var foo = provider.GetRequiredService<IFoo>();

        Assert.IsType<AnotherFoo>(foo);
    }


    [Fact]
    public void ShouldUseLast()
    {
        var container = new ServiceContainer(options => options.WithMicrosoftSettings());
        container.RegisterTransient<IFoo, Foo>("Foo1");
        container.RegisterTransient<IFoo, Foo>("Foo2");
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IFoo, AnotherFoo>();

        var provider = container.CreateServiceProvider(serviceCollection);

        var foo = provider.GetRequiredService<IFoo>();

        Assert.IsType<AnotherFoo>(foo);
    }

    public interface IFoo { }

    public class Foo : IFoo { }

    public class AnotherFoo : IFoo { }
}