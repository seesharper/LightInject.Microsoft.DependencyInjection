﻿/*********************************************************************************
    The MIT License (MIT)

    Copyright (c) 2024 bernhard.richter@gmail.com

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
******************************************************************************
    http://www.lightinject.net/
    http://twitter.com/bernhardrichter
******************************************************************************/
#if NET6_0_OR_GREATER
#define USE_ASYNCDISPOSABLE
#endif
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:PrefixLocalCallsWithThis", Justification = "No inheritance")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Single source file deployment.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:FileMustHaveHeader", Justification = "Custom header.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "All public members are documented.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Performance")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("MaintainabilityRules", "SA1403", Justification = "One source file")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("DocumentationRules", "SA1649", Justification = "One source file")]

namespace LightInject.Microsoft.DependencyInjection;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using global::Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extends the <see cref="IServiceCollection"/> interface.
/// </summary>
public static class LightInjectServiceCollectionExtensions
{
    /// <summary>
    /// Create a new <see cref="IServiceProvider"/> from the given <paramref name="serviceCollection"/>.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/> from which to create an <see cref="IServiceProvider"/>.</param>
    /// <returns>An <see cref="IServiceProvider"/> that is backed by an <see cref="IServiceContainer"/>.</returns>
    public static IServiceProvider CreateLightInjectServiceProvider(this IServiceCollection serviceCollection)
        => serviceCollection.CreateLightInjectServiceProvider(ContainerOptions.Default);

    /// <summary>
    /// Create a new <see cref="IServiceProvider"/> from the given <paramref name="serviceCollection"/>.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/> from which to create an <see cref="IServiceProvider"/>.</param>
    /// <param name="options">The <see cref="ContainerOptions"/> to be used when creating the <see cref="ServiceContainer"/>.</param>
    /// <returns>An <see cref="IServiceProvider"/> that is backed by an <see cref="IServiceContainer"/>.</returns>
    public static IServiceProvider CreateLightInjectServiceProvider(this IServiceCollection serviceCollection, ContainerOptions options)
    {
        var clonedOptions = options.Clone();
        clonedOptions.WithMicrosoftSettings();
        var container = new ServiceContainer(clonedOptions);
        container.ConstructorDependencySelector = new AnnotatedConstructorDependencySelector();
        container.ConstructorSelector = new AnnotatedConstructorSelector(container.CanGetInstance);
        return container.CreateServiceProvider(serviceCollection);
    }

    /// <summary>
    /// Create a new <see cref="IServiceProvider"/> from the given <paramref name="serviceCollection"/>.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/> from which to create an <see cref="IServiceProvider"/>.</param>
    /// <param name="configureOptions">A delegate used to configure <see cref="ContainerOptions"/>.</param>
    /// <returns>An <see cref="IServiceProvider"/> that is backed by an <see cref="IServiceContainer"/>.</returns>
    public static IServiceProvider CreateLightInjectServiceProvider(this IServiceCollection serviceCollection, Action<ContainerOptions> configureOptions)
    {
        var options = ContainerOptions.Default.Clone().WithMicrosoftSettings();
        configureOptions(options);
        return CreateLightInjectServiceProvider(serviceCollection, options);
    }
}

/// <summary>
/// Extends the <see cref="IServiceContainer"/> interface.
/// </summary>
public static class DependencyInjectionContainerExtensions
{
    /// <summary>
    /// Creates an <see cref="IServiceProvider"/> based on the given <paramref name="serviceCollection"/>.
    /// </summary>
    /// <param name="container">The target <see cref="IServiceContainer"/>.</param>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/> that contains information about the services to be registered.</param>
    /// <returns>A configured <see cref="IServiceProvider"/>.</returns>
    public static IServiceProvider CreateServiceProvider(this IServiceContainer container, IServiceCollection serviceCollection)
    {
        if (container.AvailableServices.Any(sr => sr.ServiceType == typeof(IServiceProvider)))
        {
            throw new InvalidOperationException("CreateServiceProvider can only be called once per IServiceContainer instance.");
        }

        var rootScope = container.BeginScope();
        rootScope.Completed += (a, s) => container.Dispose();
        container.Register<IServiceProvider>(f => new LightInjectServiceProvider((Scope)f));
        container.RegisterSingleton<IServiceScopeFactory>(f => new LightInjectServiceScopeFactory(container));
        container.RegisterSingleton<IServiceProviderIsService>(factory => new LightInjectIsServiceProviderIsService((serviceType, serviceName) => container.CanGetInstance(serviceType, serviceName)));
        container.RegisterSingleton<IServiceProviderIsKeyedService>(factory => new LightInjectIsServiceProviderIsService((serviceType, serviceName) => container.CanGetInstance(serviceType, serviceName)));
        RegisterServices(container, rootScope, serviceCollection);
        return new LightInjectServiceScope(rootScope).ServiceProvider;
    }

    private static void RegisterServices(IServiceContainer container, Scope rootScope, IServiceCollection serviceCollection)
    {
        var registrations = serviceCollection.Select(d => CreateServiceRegistration(d, rootScope)).ToList();

        foreach (var registration in registrations)
        {
            container.Register(registration);
        }
    }

    private static ServiceRegistration CreateServiceRegistration(ServiceDescriptor serviceDescriptor, Scope rootScope)
    {
        if (serviceDescriptor.IsKeyedService)
        {
            if (serviceDescriptor.KeyedImplementationFactory != null)
            {
                return CreateServiceRegistrationForKeyedFactoryDelegate(serviceDescriptor, rootScope);
            }

            if (serviceDescriptor.KeyedImplementationInstance != null)
            {
                return CreateServiceRegistrationForKeyedImplementationInstance(serviceDescriptor, rootScope);
            }

            return CreateServiceRegistrationForKeyedImplementationType(serviceDescriptor, rootScope);
        }
        else
        {
            if (serviceDescriptor.ImplementationFactory != null)
            {
                return CreateServiceRegistrationForFactoryDelegate(serviceDescriptor, rootScope);
            }

            if (serviceDescriptor.ImplementationInstance != null)
            {
                return CreateServiceRegistrationForImplementationInstance(serviceDescriptor, rootScope);
            }

            return CreateServiceRegistrationForImplementationType(serviceDescriptor, rootScope);
        }
    }

    private static ServiceRegistration CreateServiceRegistrationForImplementationType(ServiceDescriptor serviceDescriptor, Scope rootScope)
    {
        ServiceRegistration registration = CreateBasicServiceRegistration(serviceDescriptor, rootScope);
        registration.ImplementingType = serviceDescriptor.ImplementationType;
        return registration;
    }

    private static ServiceRegistration CreateServiceRegistrationForKeyedImplementationType(ServiceDescriptor serviceDescriptor, Scope rootScope)
    {
        ServiceRegistration registration = CreateBasicServiceRegistration(serviceDescriptor, rootScope);
        registration.ImplementingType = serviceDescriptor.KeyedImplementationType;
        registration.ServiceName = serviceDescriptor.ServiceKey.ToString();
        return registration;
    }

    private static ServiceRegistration CreateServiceRegistrationForImplementationInstance(ServiceDescriptor serviceDescriptor, Scope rootScope)
    {
        ServiceRegistration registration = CreateBasicServiceRegistration(serviceDescriptor, rootScope);
        registration.Value = serviceDescriptor.ImplementationInstance;
        return registration;
    }

    private static ServiceRegistration CreateServiceRegistrationForKeyedImplementationInstance(ServiceDescriptor serviceDescriptor, Scope rootScope)
    {
        ServiceRegistration registration = CreateBasicServiceRegistration(serviceDescriptor, rootScope);
        registration.Value = serviceDescriptor.KeyedImplementationInstance;
        LightInjectServiceProvider.KeyedServiceTypeCache.TryAdd(serviceDescriptor.ServiceType, serviceDescriptor.ServiceKey.GetType());
        registration.ServiceName = serviceDescriptor.ServiceKey.ToString();
        return registration;
    }

    private static ServiceRegistration CreateServiceRegistrationForFactoryDelegate(ServiceDescriptor serviceDescriptor, Scope rootScope)
    {
        ServiceRegistration registration = CreateBasicServiceRegistration(serviceDescriptor, rootScope);
        registration.FactoryExpression = CreateFactoryDelegate(serviceDescriptor);
        return registration;
    }

    private static ServiceRegistration CreateServiceRegistrationForKeyedFactoryDelegate(ServiceDescriptor serviceDescriptor, Scope rootScope)
    {
        ServiceRegistration registration = CreateBasicServiceRegistration(serviceDescriptor, rootScope);
        registration.FactoryExpression = CreateKeyedFactoryDelegate(serviceDescriptor);
        LightInjectServiceProvider.KeyedServiceTypeCache.TryAdd(serviceDescriptor.ServiceType, serviceDescriptor.ServiceKey.GetType());
        registration.FactoryType = FactoryType.ServiceWithServiceKey;
        return registration;
    }

    private static ServiceRegistration CreateBasicServiceRegistration(ServiceDescriptor serviceDescriptor, Scope rootScope)
    {
        ServiceRegistration registration = new()
        {
            Lifetime = ResolveLifetime(serviceDescriptor, rootScope),
            ServiceType = serviceDescriptor.ServiceType,
        };
        if (serviceDescriptor.IsKeyedService)
        {
            registration.ServiceName = serviceDescriptor.ServiceKey.ToString();
        }

        return registration;
    }

    private static ILifetime ResolveLifetime(ServiceDescriptor serviceDescriptor, Scope rootScope)
    {
        ILifetime lifetime = null;

        switch (serviceDescriptor.Lifetime)
        {
            case ServiceLifetime.Scoped:
                lifetime = new PerScopeLifetime();
                break;
            case ServiceLifetime.Singleton:
                lifetime = new PerRootScopeLifetime(rootScope);
                break;
            case ServiceLifetime.Transient:
                lifetime = NeedsTracking(serviceDescriptor) ? new PerRequestLifeTime() : null;
                break;
        }

        return lifetime;
    }

    private static bool NeedsTracking(ServiceDescriptor serviceDescriptor)
    {
        if (typeof(IDisposable).IsAssignableFrom(serviceDescriptor.ServiceType))
        {
            return true;
        }

        if (serviceDescriptor.IsKeyedService)
        {
            if (serviceDescriptor.KeyedImplementationType != null && typeof(IDisposable).IsAssignableFrom(serviceDescriptor.KeyedImplementationType))
            {
                return true;
            }
        }
        else if (serviceDescriptor.ImplementationType != null && typeof(IDisposable).IsAssignableFrom(serviceDescriptor.ImplementationType))
        {
            return true;
        }

        return false;
    }

    private static Delegate CreateFactoryDelegate(ServiceDescriptor serviceDescriptor)
    {
        var openGenericMethod = typeof(DependencyInjectionContainerExtensions).GetTypeInfo().GetDeclaredMethod("CreateTypedFactoryDelegate");
        var closedGenericMethod = openGenericMethod.MakeGenericMethod(serviceDescriptor.ServiceType.UnderlyingSystemType);
        return (Delegate)closedGenericMethod.Invoke(null, [serviceDescriptor]);
    }

#pragma warning disable IDE0051
    private static Func<IServiceFactory, T> CreateTypedFactoryDelegate<T>(ServiceDescriptor serviceDescriptor)
        => serviceFactory => (T)serviceDescriptor.ImplementationFactory(new LightInjectServiceProvider((Scope)serviceFactory));
#pragma warning restore IDE0051

    private static Delegate CreateKeyedFactoryDelegate(ServiceDescriptor serviceDescriptor)
    {
        var openGenericMethod = typeof(DependencyInjectionContainerExtensions).GetTypeInfo().GetDeclaredMethod("CreateTypedKeyedFactoryDelegate");
        var closedGenericMethod = openGenericMethod.MakeGenericMethod(serviceDescriptor.ServiceType.UnderlyingSystemType);
        return (Delegate)closedGenericMethod.Invoke(null, [serviceDescriptor]);
    }

#pragma warning disable IDE0051
    private static Func<IServiceFactory, string, T> CreateTypedKeyedFactoryDelegate<T>(ServiceDescriptor serviceDescriptor)
    {
        return (serviceFactory, serviceName) =>
        {
            LightInjectServiceProvider.KeyedServiceTypeCache.TryGetValue(serviceDescriptor.ServiceType, out var serviceKeyType);
            object key;
            if (serviceKeyType.IsEnum)
            {
                key = Enum.Parse(serviceKeyType, serviceName);
            }
            else if (serviceKeyType == typeof(int))
            {
                key = int.Parse(serviceName, CultureInfo.InvariantCulture);
            }
            else if (serviceKeyType == typeof(string))
            {
                key = serviceName;
            }
            else
            {
                try
                {
                    key = Convert.ChangeType(serviceName, serviceKeyType, CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Unable to convert service key '{serviceName}' to type '{serviceKeyType}'.", ex);
                }
            }

            return (T)serviceDescriptor.KeyedImplementationFactory(new LightInjectServiceProvider((Scope)serviceFactory), key);
        };
    }
#pragma warning restore IDE0051
}

/// <summary>
/// Extends the <see cref="ContainerOptions"/> class.
/// </summary>
public static class ContainerOptionsExtensions
{
    /// <summary>
    /// Sets up the <see cref="ContainerOptions"/> to be compliant with the conventions used in Microsoft.Extensions.DependencyInjection.
    /// </summary>
    /// <param name="options">The target <see cref="ContainerOptions"/>.</param>
    /// <returns><see cref="ContainerOptions"/>.</returns>
    public static ContainerOptions WithMicrosoftSettings(this ContainerOptions options)
    {
        options.EnablePropertyInjection = false;
        options.EnableCurrentScope = false;
        options.EnableOptionalArguments = true;
        options.EnableMicrosoftCompatibility = true;
        return options;
    }

    /// <summary>
    /// Creates a clone of the given paramref name="containerOptions".
    /// </summary>
    /// <param name="containerOptions">The <see cref="ContainerOptions"/> for which to create a clone.</param>
    /// <returns>A clone of the given paramref name="containerOptions".</returns>
    public static ContainerOptions Clone(this ContainerOptions containerOptions) => new()
    {
        DefaultServiceSelector = containerOptions.DefaultServiceSelector,
        EnableCurrentScope = containerOptions.EnableCurrentScope,
        EnablePropertyInjection = containerOptions.EnablePropertyInjection,
        EnableVariance = containerOptions.EnableVariance,
        LogFactory = containerOptions.LogFactory,
        VarianceFilter = containerOptions.VarianceFilter,
        EnableOptionalArguments = containerOptions.EnableOptionalArguments,
        EnableMicrosoftCompatibility = containerOptions.EnableMicrosoftCompatibility,
    };
}

/// <summary>
/// Creates a LightInject container builder.
/// </summary>
public class LightInjectServiceProviderFactory : IServiceProviderFactory<IServiceContainer>
{
    private readonly Func<IServiceContainer> containerFactory;

    private IServiceCollection services;

    /// <summary>
    /// Initializes a new instance of the <see cref="LightInjectServiceProviderFactory"/> class.
    /// </summary>
    public LightInjectServiceProviderFactory()
        : this(ContainerOptions.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LightInjectServiceProviderFactory"/> class.
    /// </summary>
    /// <param name="options">The <see cref="ContainerOptions"/> to be used when creating the <see cref="ServiceContainer"/>.</param>
    public LightInjectServiceProviderFactory(ContainerOptions options)
    {
        var clonedOptions = options.Clone();
        clonedOptions.WithMicrosoftSettings();
        containerFactory = () => new ServiceContainer(clonedOptions);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LightInjectServiceProviderFactory"/> class.
    /// </summary>
    /// <param name="configureOptions">A delegate used to configure <see cref="ContainerOptions"/>.</param>
    public LightInjectServiceProviderFactory(Action<ContainerOptions> configureOptions)
    {
        var options = ContainerOptions.Default.Clone().WithMicrosoftSettings();
        configureOptions(options);
        containerFactory = () => new ServiceContainer(options);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LightInjectServiceProviderFactory"/> class.
    /// </summary>
    /// <param name="serviceContainer">The <see cref="IServiceContainer"/> to be used.</param>
    public LightInjectServiceProviderFactory(IServiceContainer serviceContainer)
        => containerFactory = () => serviceContainer;

    /// <inheritdoc/>
    public IServiceContainer CreateBuilder(IServiceCollection services)
    {
        this.services = services;
        return containerFactory();
    }

    /// <inheritdoc/>
    public IServiceProvider CreateServiceProvider(IServiceContainer containerBuilder)
        => containerBuilder.CreateServiceProvider(services);
}

/// <summary>
/// An <see cref="IServiceProvider"/> that uses LightInject as the underlying container.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="LightInjectServiceProvider"/> class.
/// </remarks>
/// <param name="scope">The <see cref="Scope"/> from which this service provider requests services.</param>
#if USE_ASYNCDISPOSABLE
internal class LightInjectServiceProvider(Scope scope) : IServiceProvider, ISupportRequiredService, IKeyedServiceProvider, IDisposable, IAsyncDisposable
#else
internal class LightInjectServiceProvider(Scope scope) : IServiceProvider, ISupportRequiredService, IDisposable
#endif
{
    private bool isDisposed = false;

    public static ConcurrentDictionary<Type, Type> KeyedServiceTypeCache { get; } = new ConcurrentDictionary<Type, Type>();

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        isDisposed = true;
        scope.Dispose();
    }
#if USE_ASYNCDISPOSABLE

    public ValueTask DisposeAsync()
    {
        if (isDisposed)
        {
            return ValueTask.CompletedTask;
        }

        isDisposed = true;

        return scope.DisposeAsync();
    }

    public object GetKeyedService(Type serviceType, object serviceKey)
    {
        if (serviceKey != null)
        {
            KeyedServiceTypeCache.AddOrUpdate(serviceType, serviceKey.GetType(), (t, _) => serviceKey.GetType());
        }

        return scope.TryGetInstance(serviceType, serviceKey?.ToString());
    }

    public object GetRequiredKeyedService(Type serviceType, object serviceKey)
    {
        if (serviceKey != null)
        {
            KeyedServiceTypeCache.AddOrUpdate(serviceType, serviceKey.GetType(), (t, _) => serviceKey.GetType());
        }

        return scope.GetInstance(serviceType, serviceKey?.ToString());
    }
#endif

    /// <summary>
    /// Gets an instance of the given <paramref name="serviceType"/>.
    /// </summary>
    /// <param name="serviceType">The service type to return.</param>
    /// <returns>An instance of the given <paramref name="serviceType"/>.
    /// Throws an exception if it cannot be created.</returns>
    public object GetRequiredService(Type serviceType)
        => scope.GetInstance(serviceType);

    /// <summary>
    /// Gets an instance of the given <paramref name="serviceType"/>.
    /// </summary>
    /// <param name="serviceType">The service type to return.</param>
    /// <returns>An instance of the given <paramref name="serviceType"/>.</returns>
    public object GetService(Type serviceType)
        => scope.TryGetInstance(serviceType);
}

/// <summary>
/// An <see cref="IServiceScopeFactory"/> that uses an <see cref="IServiceContainer"/> to create new scopes.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="LightInjectServiceScopeFactory"/> class.
/// </remarks>
/// <param name="container">The <see cref="IServiceContainer"/> used to create new scopes.</param>
internal class LightInjectServiceScopeFactory(IServiceContainer container) : IServiceScopeFactory
{

    /// <inheritdoc/>
    public IServiceScope CreateScope()
        => new LightInjectServiceScope(container.BeginScope());
}

/// <summary>
/// An <see cref="IServiceScope"/> implementation that wraps a <see cref="Scope"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="LightInjectServiceScope"/> class.
/// </remarks>
/// <param name="scope">The <see cref="Scope"/> wrapped by this class.</param>
#if USE_ASYNCDISPOSABLE
internal class LightInjectServiceScope(Scope scope) : IServiceScope, IAsyncDisposable
#else
internal class LightInjectServiceScope(Scope scope) : IServiceScope
#endif
{
    public IServiceProvider ServiceProvider { get; } = new LightInjectServiceProvider(scope);

    /// <inheritdoc/>
    public void Dispose() => scope.Dispose();

#if USE_ASYNCDISPOSABLE
    /// <inheritdoc/>
    public ValueTask DisposeAsync() => scope.DisposeAsync();
#endif
}

/// <summary>
/// An <see cref="ILifetime"/> implementation that makes it possible to mimic the notion of a root scope.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PerRootScopeLifetime"/> class.
/// </remarks>
/// <param name="rootScope">The root <see cref="Scope"/>.</param>
[LifeSpan(30)]
internal class PerRootScopeLifetime(Scope rootScope) : ILifetime, ICloneableLifeTime
{
    private readonly object syncRoot = new();
    private object instance;

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public object GetInstance(Func<object> createInstance, Scope scope)
        => throw new NotImplementedException("Uses optimized non closing method");

    /// <inheritdoc/>
    public ILifetime Clone()
        => new PerRootScopeLifetime(rootScope);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable IDE0060
    public object GetInstance(GetInstanceDelegate createInstance, Scope scope, object[] arguments)
    {
#pragma warning restore IDE0060
        if (instance != null)
        {
            return instance;
        }

        lock (syncRoot)
        {
            if (instance == null)
            {
                instance = createInstance(arguments, rootScope);
                RegisterForDisposal(instance);
            }
        }

        return instance;
    }

    private void RegisterForDisposal(object instance)
    {
        if (instance is IDisposable disposable)
        {
            rootScope.TrackInstance(disposable);
        }
        else if (instance is IAsyncDisposable asyncDisposable)
        {
            rootScope.TrackInstance(asyncDisposable);
        }
    }
}

internal class LightInjectIsServiceProviderIsService(Func<Type, string, bool> canGetService) : IServiceProviderIsKeyedService
{
    public bool IsKeyedService(Type serviceType, object serviceKey)
    {
        if (serviceType.IsGenericTypeDefinition)
        {
            return false;
        }

        return canGetService(serviceType, serviceKey?.ToString() ?? string.Empty);
    }

    public bool IsService(Type serviceType)
    {
        if (serviceType.IsGenericTypeDefinition)
        {
            return false;
        }

        return canGetService(serviceType, string.Empty);
    }
}

/// <summary>
/// A <see cref="ConstructorDependencySelector"/> that looks for the <see cref="FromKeyedServicesAttribute"/> 
/// to determine the name of service to be injected.
/// </summary>
public class AnnotatedConstructorDependencySelector : ConstructorDependencySelector
{
    /// <summary>
    /// Selects the constructor dependencies for the given <paramref name="constructor"/>.
    /// </summary>
    /// <param name="constructor">The <see cref="ConstructionInfo"/> for which to select the constructor dependencies.</param>
    /// <returns>A list of <see cref="ConstructorDependency"/> instances that represents the constructor
    /// dependencies for the given <paramref name="constructor"/>.</returns>
    public override IEnumerable<ConstructorDependency> Execute(ConstructorInfo constructor)
    {
        var constructorDependencies = base.Execute(constructor).ToArray();
        foreach (var constructorDependency in constructorDependencies)
        {
            var injectAttribute =
                constructorDependency.Parameter.GetFromKeyedServicesAttributes<FromKeyedServicesAttribute>().FirstOrDefault();
            if (injectAttribute != null)
            {
                constructorDependency.ServiceName = injectAttribute.Key.ToString();
            }
        }

        return constructorDependencies;
    }
}

/// <summary>
/// A <see cref="IConstructorSelector"/> implementation that uses information 
/// from the <see cref="FromKeyedServicesAttribute"/> to determine if a given service can be resolved.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AnnotatedConstructorSelector"/> class.
/// </remarks>
/// <param name="canGetInstance">A function delegate that determines if a service type can be resolved.</param>
public class AnnotatedConstructorSelector(Func<Type, string, bool> canGetInstance) : MostResolvableConstructorSelector(canGetInstance)
{

    /// <summary>
    /// Gets the service name based on the given <paramref name="parameter"/>.
    /// </summary>
    /// <param name="parameter">The <see cref="ParameterInfo"/> for which to get the service name.</param>
    /// <returns>The name of the service for the given <paramref name="parameter"/>.</returns>
    protected override string GetServiceName(ParameterInfo parameter)
    {
        var injectAttribute = parameter.GetFromKeyedServicesAttributes<FromKeyedServicesAttribute>().FirstOrDefault();
        return injectAttribute != null ? injectAttribute.Key.ToString() : base.GetServiceName(parameter);
    }
}

internal static class AttributeExtensions
{
    internal static T[] GetFromKeyedServicesAttributes<T>(this ParameterInfo member) where T : FromKeyedServicesAttribute
    {
        return member.GetCustomAttributes(inherit: true)
            .OfType<T>()
            .ToArray();
    }
}