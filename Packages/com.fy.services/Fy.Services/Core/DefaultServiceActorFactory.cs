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
            T[] existingServices = Object.FindObjectsByType<T>(FindObjectsInactive.Exclude);

            if (existingServices.Length == 0)
            {
                GameObject serviceObject = new GameObject(typeof(T).Name);
                T createdService = serviceObject.AddComponent<T>();
                HandlePersistenceState(createdService);

                return createdService;
            }

            if (existingServices.Length > 1)
            {
                Debug.LogError($"Multiple '{typeof(T).Name}' service instances found " +
                               $"({existingServices.Length}). Keeping the first; remove the extras.");
            }

            T existingService = existingServices[0];

            for (int i = 1; i < existingServices.Length; i++)
            {
                if (existingServices[i].GetEntityId() < existingService.GetEntityId())
                {
                    existingService = existingServices[i];
                }
            }

            HandlePersistenceState(existingService);

            return existingService;
        }

        private static void HandlePersistenceState(T service)
        {
            if (!IsPersistent)
            {
                return;
            }

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
