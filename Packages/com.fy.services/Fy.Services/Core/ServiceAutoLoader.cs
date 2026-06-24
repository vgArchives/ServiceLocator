using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Fy.Services
{
    public static class ServiceAutoLoader
    {
        private static Type[] _serviceTypes;
        private static (Type serviceInterface, Type implementation)[] _services;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void AutoRegisterAll()
        {
            foreach ((Type serviceInterface, Type cls) in GetServices())
            {
                if (cls.GetCustomAttribute<DisableDefaultFactoryAttribute>() != null)
                {
                    continue;
                }

                RegisterDefaultFactory(serviceInterface, cls);
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ValidateRequiredServices();
            ValidatePersistentServices();
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void PreloadAll()
        {
            foreach ((Type serviceInterface, Type cls) in GetServices())
            {
                if (cls.GetCustomAttribute<PreloadServiceAttribute>() == null)
                {
                    continue;
                }

                Preload(serviceInterface);
            }
        }
        
        internal static void ValidateRequiredServices()
        {
            foreach (Type type in GetServiceTypes())
            {
                if (!type.IsInterface
                 || type.GetCustomAttribute<RequiredServiceAttribute>() == null)
                {
                    continue;
                }

                if (!ServiceLocator.HasFactory(type))
                {
                    Debug.LogError($"Required service '{type.Name}' has no registered factory; " +
                                   $"GetChecked will fail at runtime. Provide an implementation or " +
                                   $"register a factory for it.");
                }
            }
        }

        internal static void ValidatePersistentServices()
        {
            foreach (Type type in GetServiceTypes())
            {
                if (type.IsAbstract
                 || type.IsInterface
                 || type.GetCustomAttribute<PersistentServiceAttribute>() == null)
                {
                    continue;
                }

                if (!typeof(MonoBehaviour).IsAssignableFrom(type))
                {
                    Debug.LogError($"[PersistentService] on '{type.Name}' is ignored \u2014 only MonoBehaviour " +
                                   $"services need it; non-MonoBehaviour services already persist across scenes.");
                }
            }
        }

        private static Type[] GetServiceTypes()
        {
            if (_serviceTypes != null)
            {
                return _serviceTypes;
            }

            var types = new List<Type>();

            foreach (Assembly asm in GetServiceAssemblies())
            {
                types.AddRange(SafeGetTypes(asm));
            }

            _serviceTypes = types.ToArray();
            return _serviceTypes;
        }

        private static (Type serviceInterface, Type implementation)[] GetServices()
        {
            if (_services != null)
            {
                return _services;
            }

            _services = FindServices().ToArray();
            return _services;
        }

        private static IEnumerable<Assembly> GetServiceAssemblies()
        {
            Assembly coreAssembly = typeof(IService).Assembly;
            string coreName = coreAssembly.GetName().Name;

            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => asm == coreAssembly
                              || asm.GetReferencedAssemblies().Any(r => r.Name == coreName));
        }

        private static IEnumerable<(Type serviceInterface, Type implementation)> FindServices()
        {
            foreach (Type cls in GetServiceTypes())
            {
                if (cls.IsAbstract || cls.IsInterface)
                {
                    continue;
                }

                Type serviceInterface = cls.GetInterfaces().FirstOrDefault(i =>
                    i != typeof(IService) &&
                    typeof(IService).IsAssignableFrom(i) &&
                    i.GetCustomAttribute<AbstractServiceAttribute>() == null);

                if (serviceInterface == null)
                {
                    continue;
                }

                yield return (serviceInterface, cls);
            }
        }

        private static void RegisterDefaultFactory(Type serviceInterface, Type cls)
        {
            if (ServiceLocator.HasFactory(serviceInterface))
            {
                return;
            }

            bool isUnityComponent = typeof(MonoBehaviour).IsAssignableFrom(cls);

            Type openFactory = isUnityComponent
                ? typeof(DefaultServiceActorFactory<>)
                : typeof(DefaultServiceFactory<>);

            Type closedFactory = openFactory.MakeGenericType(cls);
            var factoryInstance = (IServiceFactory)closedFactory
                .GetField("Instance", BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);

            ServiceLocator.SetFactory(serviceInterface, factoryInstance);
        }

        private static void Preload(Type serviceInterface)
        {
            IService service = ServiceLocator.GetService(serviceInterface);

            if (service == null && serviceInterface.GetCustomAttribute<RequiredServiceAttribute>() != null)
            {
                throw new InvalidOperationException($"Required service '{serviceInterface.Name}' missing.");
            }
        }

        private static Type[] SafeGetTypes(Assembly asm)
        {
            try
            {
                return asm.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null).ToArray();
            }
        }
    }
}
