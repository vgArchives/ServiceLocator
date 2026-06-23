using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Fy.Services
{
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
        
        public static void SetFactory<T>(IServiceFactory factory) where T : class, IService
        {
            SetFactory(typeof(T), factory);
        }

        public static bool HasFactory<T>() where T : class, IService
        {
            return HasFactory(typeof(T));
        }

        public static bool TryGet<T>(out T service) where T : class, IService
        {
            service = (T)GetService(typeof(T));
            return service != null;
        }

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

        internal static void SetFactory(Type type, IServiceFactory factory)
        {
            Initialize(type, out ServiceWrapper service);
            service.Factory = factory;
        }

        internal static bool HasFactory(Type type)
        {
            return Services.TryGetValue(type, out ServiceWrapper service) && service.Factory != null;
        }

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
                if (serviceWrapper.Factory.ShouldSetService)
                {
                    serviceWrapper.Value = validService;
                }

                return validService;
            }

            createdService?.Dispose();
            return null;
        }

        internal static IEnumerable<ServiceSnapshot> EnumerateServices()
        {
            foreach (KeyValuePair<Type, ServiceWrapper> entry in Services)
            {
                ServiceWrapper wrapper = entry.Value;
                yield return new ServiceSnapshot(entry.Key, wrapper.Value, wrapper.Factory,
                    wrapper.IsDynamic, wrapper.IsRequired);
            }
        }

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