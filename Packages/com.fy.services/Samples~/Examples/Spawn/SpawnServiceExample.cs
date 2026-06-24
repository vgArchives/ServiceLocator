using Fy.Services;
using UnityEngine;

namespace Fy.Services.Examples
{
    // Drop this on a GameObject and press Play.
    //
    // Resolve the concrete service and use it. The example also shows the other
    // half of [AbstractService]: asking for the base ISpawnService gives you
    // nothing, on purpose.
    public sealed class SpawnServiceExample : MonoBehaviour
    {
        private void Start()
        {
            if (!ServiceLocator.TryGet(out IEnemySpawnService spawner))
            {
                Debug.LogWarning("Enemy spawn service was not available.");
                return;
            }

            spawner.Spawn();

            // The base interface is [AbstractService], so this resolves to
            // nothing. That's the whole point of the attribute.
            bool baseResolves = ServiceLocator.TryGet(out ISpawnService _);
            Debug.Log($"Concrete service resolved. Base ISpawnService resolves: {baseResolves}");
        }
    }
}
