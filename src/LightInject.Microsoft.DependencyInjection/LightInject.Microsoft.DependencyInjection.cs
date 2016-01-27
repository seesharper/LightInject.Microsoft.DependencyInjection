/*********************************************************************************
    The MIT License (MIT)

    Copyright (c) 2016 bernhard.richter@gmail.com

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
    LightInject.Microsoft.DependencyInjection version 1.0.1
    http://www.lightinject.net/
    http://twitter.com/bernhardrichter
******************************************************************************/

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:PrefixLocalCallsWithThis", Justification = "No inheritance")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Single source file deployment.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:FileMustHaveHeader", Justification = "Custom header.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "All public members are documented.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Performance")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("MaintainabilityRules", "SA1403", Justification = "One source file")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("DocumentationRules", "SA1649", Justification = "One source file")]

namespace LightInject.Microsoft.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Remoting.Messaging;
    using global::Microsoft.Extensions.DependencyInjection;

    public static class DependencyInjectionContainerExtensions
    {
        public static IServiceProvider CreateServiceProvider(this IServiceContainer container, IServiceCollection serviceCollection)
        {
            container.Register<IServiceProvider>(factory => new LightInjectServiceProvider(container), new PerContainerLifetime());
            container.Register<IServiceScopeFactory>(factory => new LightInjectServiceScopeFactory(container), new PerContainerLifetime());
            Dictionary<Type, List<ServiceRegistration>> services = new Dictionary<Type, List<ServiceRegistration>>();
            foreach (var serviceDescriptor in serviceCollection)
            {
                var registration = CreateServiceRegistration(container, serviceDescriptor);
                List<ServiceRegistration> existingRegistrations;
                if (services.TryGetValue(serviceDescriptor.ServiceType, out existingRegistrations))
                {
                    foreach (var existingRegistration in existingRegistrations)
                    {
                        existingRegistration.ServiceName = Guid.NewGuid().ToString();
                    }
                    existingRegistrations.Add(registration);
                }
                else
                {
                    existingRegistrations = new List<ServiceRegistration>();
                    existingRegistrations.Add(registration);
                    services.Add(serviceDescriptor.ServiceType, existingRegistrations);
                }
            }

            var registrations = services.Values.SelectMany(s => s);
            foreach (var registration in registrations)
            {
                container.Register(registration);
            }

            return container.GetInstance<IServiceProvider>();
        }

        private static ServiceRegistration CreateServiceRegistration(IServiceContainer container, ServiceDescriptor serviceDescriptor)
        {
            if (serviceDescriptor.ImplementationFactory != null)
            {
                return CreateServiceRegistrationForFactoryDelegate(container, serviceDescriptor);
            }
            if (serviceDescriptor.ImplementationInstance != null)
            {
                return CreateServiceRegistrationForInstance(serviceDescriptor);
            }
            return CreateServiceRegistrationServiceType(serviceDescriptor);

        }

        private static ServiceRegistration CreateServiceRegistrationServiceType(ServiceDescriptor serviceDescriptor)
        {
            ServiceRegistration registration = new ServiceRegistration();
            registration.ServiceType = serviceDescriptor.ServiceType;
            registration.ImplementingType = serviceDescriptor.ImplementationType;
            registration.Lifetime = ResolveLifetime(serviceDescriptor);
            registration.ServiceName = string.Empty;
            return registration;
        }

        private static ServiceRegistration CreateServiceRegistrationForInstance(ServiceDescriptor serviceDescriptor)
        {
            ServiceRegistration registration = new ServiceRegistration();
            registration.ServiceType = serviceDescriptor.ServiceType;
            registration.ServiceName = string.Empty;
            registration.Value = serviceDescriptor.ImplementationInstance;
            return registration;
        }


        private static ServiceRegistration CreateServiceRegistrationForFactoryDelegate(IServiceContainer container, ServiceDescriptor serviceDescriptor)
        {
            ServiceRegistration registration = new ServiceRegistration();
            registration.Lifetime = ResolveLifetime(serviceDescriptor);
            registration.FactoryExpression = CreateFactoryDelegate(serviceDescriptor);
            registration.ServiceType = serviceDescriptor.ServiceType;
            registration.ServiceName = string.Empty;
            return registration;
        }
      
        private static ILifetime ResolveLifetime(ServiceDescriptor serviceDescriptor)
        {
            switch (serviceDescriptor.Lifetime)
            {
                case ServiceLifetime.Scoped:
                    return new AspNetPerScopeLifetime();
                case ServiceLifetime.Singleton:
                    return new PerContainerLifetime();
                case ServiceLifetime.Transient:
                    return new PerRequestLifeTime();
            }
            return null;
        }

        private static Delegate CreateFactoryDelegate(ServiceDescriptor serviceDescriptor)
        {
            var openGenericMethod = typeof(DependencyInjectionContainerExtensions).GetTypeInfo().GetMethod("CreateTypedFactoryDelegate", BindingFlags.Static | BindingFlags.NonPublic);
            var closedGenericMethod = openGenericMethod.MakeGenericMethod(serviceDescriptor.ServiceType);
            return (Delegate)closedGenericMethod.Invoke(null, new[] { serviceDescriptor });
        }

        private static Func<IServiceContainer, T> CreateTypedFactoryDelegate<T>(ServiceDescriptor serviceDescriptor)
        {
            return container => (T)serviceDescriptor.ImplementationFactory(container.GetInstance<IServiceProvider>());
        }

    }


    /// <summary>
    /// A LightInject <see cref="IServiceProvider"/> to be used by ASP.NET Core applications
    /// </summary>
    public class LightInjectServiceProvider : IServiceProvider, IDisposable
    {
        private readonly IServiceContainer serviceContainer;
        protected readonly Scope Scope;
        protected bool IsDisposed;


        public LightInjectServiceProvider(IServiceContainer serviceContainer)
        {
            this.serviceContainer = serviceContainer;
            Scope = serviceContainer.BeginScope();
        }

        /// <summary>
        /// Gets an instance of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to return.</param>
        /// <returns>An instance of the given <see cref="serviceType"/>.</returns>
        public object GetService(Type serviceType)
        {
            return serviceContainer.TryGetInstance(serviceType);
        }

        
        public virtual void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                Scope.Dispose();
                serviceContainer.Dispose();
            }            
        }       
    }

    public class ChildServiceProvider : LightInjectServiceProvider
    {
        public ChildServiceProvider(IServiceContainer serviceContainer) : base(serviceContainer)
        {
            
        }

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                Scope.Dispose();
            }
        }
    }

    public class LightInjectServiceScopeFactory : IServiceScopeFactory
    {
        private readonly IServiceContainer container;

        public LightInjectServiceScopeFactory(IServiceContainer container)
        {
            this.container = container;
        }

        public IServiceScope CreateScope()
        {
            return new LightInjectServiceScope(GetChildContainer());
        }

        private IServiceContainer GetChildContainer()
        {
            // TODO: Replace this with a container pool
            var childContainer = new ServiceContainer();
            container.ScopeManagerProvider = new PerLogicalCallContextScopeManagerProvider();
            foreach (var registration in container.AvailableServices)
            {
                childContainer.Register(registration);
            }
            return childContainer;
        }
    }


    public class LightInjectServiceScope : IServiceScope
    {
        public LightInjectServiceScope(IServiceContainer container)
        {
            ServiceProvider = new ChildServiceProvider(container);
        }

        public void Dispose()
        {
            ((IDisposable)ServiceProvider).Dispose();
        }

        public IServiceProvider ServiceProvider { get; }
    }

    public class AspNetPerScopeLifetime : ILifetime
    {
        private readonly ThreadSafeDictionary<Scope, object> instances = new ThreadSafeDictionary<Scope, object>();

        /// <summary>
        /// Returns the same service instance within the current <see cref="Scope"/>.
        /// </summary>
        /// <param name="createInstance">The function delegate used to create a new service instance.</param>
        /// <param name="scope">The <see cref="Scope"/> of the current service request.</param>
        /// <returns>The requested services instance.</returns>
        public object GetInstance(Func<object> createInstance, Scope scope)
        {
            if (scope == null)
            {
                return createInstance();
            }

            return instances.GetOrAdd(scope, s => CreateScopedInstance(s, createInstance));
        }

        private static void RegisterForDisposal(Scope scope, object instance)
        {
            var disposable = instance as IDisposable;
            if (disposable != null)
            {
                scope.TrackInstance(disposable);
            }
        }

        private object CreateScopedInstance(Scope scope, Func<object> createInstance)
        {

            if (scope == null)
            {
                return createInstance();
            }

            scope.Completed += OnScopeCompleted;
            var instance = createInstance();

            RegisterForDisposal(scope, instance);
            return instance;
        }

        private void OnScopeCompleted(object sender, EventArgs e)
        {
            var scope = (Scope)sender;
            scope.Completed -= OnScopeCompleted;
            object removedInstance;
            instances.TryRemove(scope, out removedInstance);
        }
    }
}
