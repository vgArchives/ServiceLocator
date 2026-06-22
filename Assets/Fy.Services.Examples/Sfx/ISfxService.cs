using Fy.Services;

namespace Fy.Services.Examples
{
    // Use this pattern when the service needs to live on a GameObject, for
    // example because it uses an AudioSource, runs coroutines, or wants
    // Update/OnDestroy callbacks.
    //
    // The implementation is a MonoBehaviour. The auto-loader notices that and
    // hands out an "actor" factory instead of the default one. When the service is
    // first requested, the factory finds an existing instance in the scene or
    // creates a GameObject for it. You never call AddComponent yourself.
    public interface ISfxService : IService
    {
        void PlaySound(string clipName);
    }
}
