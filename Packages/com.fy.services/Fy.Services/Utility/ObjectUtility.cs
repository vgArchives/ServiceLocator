using Object = UnityEngine.Object;

namespace Fy.Services
{
    /// <summary>
    /// Helpers to safely check whether a service is really usable.
    /// </summary>
    /// <remarks>
    /// A destroyed MonoBehaviour is not C# null but must be treated as gone. Unity reports this through its own
    /// <c>==</c> check, which only runs when the value is typed as a Unity object. Because services are held as
    /// <see cref="IService"/>, a plain null check would miss a destroyed one, so these helpers re-check it the right way.
    /// </remarks>
    public static class ObjectUtility
    {
        /// <summary>
        /// Whether the service is usable: not null, and not a destroyed Unity object.
        /// </summary>
        /// <param name="service">The service to check.</param>
        /// <returns>True if the service is safe to use.</returns>
        public static bool IsValid(this IService service)
        {
            if (service is Object unityObject)
            {
                return unityObject != null;
            }

            return service != null;
        }

        /// <summary>
        /// Returns the service through <paramref name="validService"/> only if it is usable.
        /// </summary>
        /// <param name="service">The service to check.</param>
        /// <param name="validService">The service if usable, otherwise null.</param>
        /// <returns>True if the service is usable.</returns>
        public static bool TryGetValid(this IService service, out IService validService)
        {
            validService = service.GetValid();
            return validService != null;
        }

        /// <summary>
        /// Returns the service if it is usable, otherwise null.
        /// </summary>
        /// <param name="service">The service to check.</param>
        /// <returns>The service, or null if it is not usable.</returns>
        public static IService GetValid(this IService service)
        {
            return service.IsValid() ? service : null;
        }
    }
}