using Object = UnityEngine.Object;

namespace Fy.Services
{
    public static class ObjectUtility
    {
        public static bool IsValid(this IService service)
        {
            if (service is Object unityObject)
            {
                return unityObject != null;
            }

            return service != null;
        }
        
        public static bool TryGetValid(this IService service, out IService validService)
        {
            validService = service.GetValid();
            return validService != null;
        }
        
        public static IService GetValid(this IService service)
        {
            return service.IsValid() ? service : null;
        }
    }
}