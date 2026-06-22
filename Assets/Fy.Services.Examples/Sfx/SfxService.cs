using UnityEngine;

namespace Fy.Services.Examples
{
    // A service that is also a MonoBehaviour. Two things worth copying here:
    //
    // 1. The constructor is private. MonoBehaviours are created by Unity, not
    //    with "new", so hiding the constructor stops anyone doing it by mistake.
    // 2. Dispose() tears down the GameObject. The locator calls Dispose() when
    //    the service goes away, so the object it created doesn't leak.
    [PersistentService]
    public sealed class SfxService : MonoBehaviour, ISfxService
    {
        private AudioSource _source;

        private SfxService() { }

        private void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
        }

        public void PlaySound(string clipName)
        {
            Debug.Log($"[Sfx] play '{clipName}'");
        }

        public void Dispose()
        {
            // The "this != null" check matters for MonoBehaviours: the C#
            // object can outlive the real Unity object after it's destroyed.
            if (this != null)
            {
                Destroy(gameObject);
            }
        }
    }
}
