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
            Initialize(typeof(T), out ServiceWrapper service);
            service.Factory = factory;
        }
        
        public static bool HasFactory<T>() where T : class, IService
        {
            return Services.TryGetValue(typeof(T), out ServiceWrapper service) && service.Factory != null;
        }
        
        public static bool TryGet<T>(out T service) where T : class, IService
        {
            service = GetService<T>();
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
            
            T value = GetService<T>(service);
            
            if (value == null)
            {
                throw new InvalidOperationException($"Required service '{typeof(T).Name}' missing.");
            }
            
            return value;
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

        private static T GetService<T>() where T : class, IService
        {
            Initialize(typeof(T), out ServiceWrapper serviceWrapper);
            return GetService<T>(serviceWrapper);
        }

        private static T GetService<T>(ServiceWrapper serviceWrapper) where T : class, IService
        {
            if (serviceWrapper.Value.TryGetValid(out IService validService))
            {
                return (T)validService;
            }

            if (serviceWrapper.Factory == null)
            {
                return null;
            }

            IService createdService = serviceWrapper.Factory.GetService();

            if (createdService.TryGetValid(out validService) && validService is T result)
            {
                if (serviceWrapper.Factory.ShouldSetService)
                {
                    serviceWrapper.Value = result;
                }
                
                return result;
            }
            
            createdService?.Dispose();
            return null;
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