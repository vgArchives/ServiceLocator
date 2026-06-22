using System.Reflection;
using UnityEngine;

namespace Fy.Services
{
    public sealed class DefaultServiceActorFactory<T> : IServiceFactory where T : MonoBehaviour, IService
    {
        public static readonly DefaultServiceActorFactory<T> Instance = new();

        private static readonly bool IsPersistent =
            typeof(T).GetCustomAttribute<PersistentServiceAttribute>() != null;

        private DefaultServiceActorFactory() { }

        public IService GetService()
        {
            T existingService = Object.FindAnyObjectByType<T>();
            if (existingService != null)
            {
                if (IsPersistent)
                {
                    PersistFound(existingService);
                }

                return existingService;
            }

            GameObject serviceObject = new GameObject(typeof(T).Name);
            T createdService = serviceObject.AddComponent<T>();

            if (IsPersistent)
            {
                Object.DontDestroyOnLoad(serviceObject);
            }

            return createdService;
        }

        private static void PersistFound(T service)
        {
            if (service.transform.parent != null)
            {
                Debug.LogError($"[PersistentService] '{typeof(T).Name}' was found as a child object and " +
                               $"cannot persist across scenes. Place it at the scene root so it survives scene loads.");
                return;
            }

            Object.DontDestroyOnLoad(service.gameObject);
        }
    }
}
