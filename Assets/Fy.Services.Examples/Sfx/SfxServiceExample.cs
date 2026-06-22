using Fy.Services;
using UnityEngine;

namespace Fy.Services.Examples
{
    // Drop this on a GameObject and press Play.
    //
    // Resolving a MonoBehaviour service looks exactly like resolving a regular
    // one. The difference is invisible to the caller: behind the scenes a
    // GameObject gets created to host the service. Look in the Hierarchy after
    // pressing Play and you'll see it appear.
    public sealed class SfxServiceExample : MonoBehaviour
    {
        private void Start()
        {
            if (!ServiceLocator.TryGet(out ISfxService sfx))
            {
                Debug.LogWarning("Sfx service was not available.");
                return;
            }

            sfx.PlaySound("Explosion");
        }
    }
}
