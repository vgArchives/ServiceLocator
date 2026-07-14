using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Fy.Services
{
    /// <summary>
    /// Scans your assemblies at startup, pairs each service interface with its implementation, and registers a
    /// default factory for it, so services work without any manual setup.
    /// </summary>
    /// <remarks>
    /// Runs automatically through Unity's startup callbacks. Only assemblies that reference this package are scanned,
    /// keeping the search fast.
    /// </remarks>
    public static class ServiceAutoLoader
    {
        private static Type[] _serviceTypes;
        private static (Type serviceInterface, Type implementation)[] _services;

        /// <summary>
        /// Registers a default factory for every discovered service. Runs at the earliest startup phase.
        /// </summary>
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

        /// <summary>
        /// Eagerly builds every service marked <see cref="PreloadServiceAttribute"/> before the first scene loads,
        /// instead of waiting for the first request.
        /// </summary>
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
        
        /// <summary>
        /// Logs an error for any <see cref="RequiredServiceAttribute"/> service that has no factory, catching the
        /// problem at startup instead of when <see cref="ServiceLocator.GetChecked{T}"/> throws later.
        /// </summary>
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

        /// <summary>
        /// Logs an error when <see cref="PersistentServiceAttribute"/> is placed on a non-MonoBehaviour service,
        /// where it has no effect.
        /// </summary>
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
