using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Fy.Services
{
    /// <summary>
    /// Central registry that hands out shared services by their interface. Not thread-safe.
    /// </summary>
    /// <remarks>
    /// Look up a service with <see cref="TryGet{T}"/> or <see cref="GetChecked{T}"/>; the locator builds it on first
    /// request through a registered <see cref="IServiceFactory"/> and reuses it afterwards. Most services are wired
    /// up automatically by the <see cref="ServiceAutoLoader"/>, so you rarely need to register anything by hand.
    /// </remarks>
    public static class ServiceLocator
    {
        private sealed class ServiceWrapper
        {
            public IService Value;
            public IServiceFactory Factory;
        
            public readonly bool IsDynamic;
            public readonly bool IsRequired;

            public ServiceWrapper(Type type)
            {
                IsDynamic = type.GetCustomAttribute<DynamicServiceAttribute>() != null;
                IsRequired = type.GetCustomAttribute<RequiredServiceAttribute>() != null;
            }
        }
        
        private static readonly Dictionary<Type, ServiceWrapper> Services = new();

        /// <summary>
        /// Registers a ready-made service instance. Fails if one is already set, unless the interface is marked
        /// <see cref="DynamicServiceAttribute"/>, in which case the previous instance is disposed and replaced.
        /// </summary>
        /// <param name="value">The service instance to store.</param>
        /// <typeparam name="T">The service interface.</typeparam>
        public static void SetService<T>(T value) where T : class, IService
        {
            Initialize(typeof(T), out ServiceWrapper service);

            if (service.Value.TryGetValid(out IService previousService))
            {
                if (!service.IsDynamic)
                {
                    Debug.LogError($"Service '{typeof(T).Name}' is already set. " +
                                   $"Mark its interface [DynamicService] to allow replacing it.");
                    return;
                }
                
                previousService.Dispose();
            }

            service.Value = value.GetValid();
        }
        
        /// <summary>
        /// Registers the factory that builds this service the first time it is requested.
        /// </summary>
        /// <param name="factory">The factory to use.</param>
        /// <typeparam name="T">The service interface.</typeparam>
        public static void SetFactory<T>(IServiceFactory factory) where T : class, IService
        {
            SetFactory(typeof(T), factory);
        }

        /// <summary>
        /// Whether a factory has been registered for this service.
        /// </summary>
        /// <typeparam name="T">The service interface.</typeparam>
        /// <returns>True if a factory is set.</returns>
        public static bool HasFactory<T>() where T : class, IService
        {
            return HasFactory(typeof(T));
        }

        /// <summary>
        /// Tries to get a service, building it on first request. Use this for optional services.
        /// </summary>
        /// <param name="service">The resolved service, or null if it could not be provided.</param>
        /// <typeparam name="T">The service interface.</typeparam>
        /// <returns>True if a service was resolved.</returns>
        public static bool TryGet<T>(out T service) where T : class, IService
        {
            service = (T)GetService(typeof(T));
            return service != null;
        }

        /// <summary>
        /// Gets a service that must exist, throwing if it is missing. Use this for services marked
        /// <see cref="RequiredServiceAttribute"/>.
        /// </summary>
        /// <typeparam name="T">The service interface.</typeparam>
        /// <returns>The resolved service, guaranteed non-null.</returns>
        public static T GetChecked<T>() where T : class, IService
        {
            Initialize(typeof(T), out ServiceWrapper service);

            if (!service.IsRequired)
            {
                Debug.LogWarning($"'{typeof(T).Name}' is not [RequiredService]; " +
                                 $"use TryGet for optional services.");
            }
            
            var value = (T)GetService(typeof(T));

            if (value == null)
            {
                throw new InvalidOperationException($"Required service '{typeof(T).Name}' missing.");
            }

            return value;
        }

        /// <summary>
        /// Registers a factory using a runtime type, letting the <see cref="ServiceAutoLoader"/> register services
        /// it found by reflection without going through the generic API.
        /// </summary>
        internal static void SetFactory(Type type, IServiceFactory factory)
        {
            Initialize(type, out ServiceWrapper service);
            service.Factory = factory;
        }

        /// <summary>
        /// Whether a factory is registered for the given runtime type.
        /// </summary>
        internal static bool HasFactory(Type type)
        {
            return Services.TryGetValue(type, out ServiceWrapper service) && service.Factory != null;
        }

        /// <summary>
        /// The single resolver every lookup funnels through: returns the cached instance, otherwise builds one from
        /// the factory, validates it, and caches it. Takes a runtime type so reflection-driven callers can resolve
        /// without a generic method.
        /// </summary>
        internal static IService GetService(Type type)
        {
            Initialize(type, out ServiceWrapper serviceWrapper);

            if (serviceWrapper.Value.TryGetValid(out IService validService))
            {
                return validService;
            }

            if (serviceWrapper.Factory == null)
            {
                return null;
            }

            IService createdService = serviceWrapper.Factory.GetService();

            if (createdService.TryGetValid(out validService) && type.IsInstanceOfType(validService))
            {
                if (serviceWrapper.Factory.ShouldCacheService)
                {
                    serviceWrapper.Value = validService;
                }

                return validService;
            }

            createdService?.Dispose();
            return null;
        }

        /// <summary>
        /// Yields a read-only snapshot of every registered service, used by the editor window.
        /// </summary>
        internal static IEnumerable<ServiceSnapshot> EnumerateServices()
        {
            foreach (KeyValuePair<Type, ServiceWrapper> entry in Services)
            {
                ServiceWrapper wrapper = entry.Value;
                yield return new ServiceSnapshot(entry.Key, wrapper.Value, wrapper.Factory,
                    wrapper.IsDynamic, wrapper.IsRequired);
            }
        }

        /// <summary>
        /// Disposes every service and clears the registry. Called when play mode ends and between tests so no state
        /// leaks across runs.
        /// </summary>
        internal static void Reset()
        {
            foreach (ServiceWrapper service in Services.Values)
            {
                if (service.Value.TryGetValid(out IService value))
                {
                    value.Dispose();
                }

                service.Value = null;
            }

            Services.Clear();
        }

        private static void Initialize(Type type, out ServiceWrapper serviceWrapper)
        {
            if (Services.TryGetValue(type, out serviceWrapper))
            {
                return;
            }
            
            serviceWrapper = new ServiceWrapper(type);
            Services[type] = serviceWrapper;
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void CleanupOnPlayModeExit()
        {
            UnityEditor.EditorApplication.playModeStateChanged += state =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                {
                    Reset();
                }
            };
        }
#endif
    }
}