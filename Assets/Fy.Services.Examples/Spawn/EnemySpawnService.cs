using UnityEngine;

namespace Fy.Services.Examples
{
    // Implements the concrete IEnemySpawnService (and, through it, the shared
    // ISpawnService contract). The locator keys services by the interface you
    // ask for, so this resolves under IEnemySpawnService, never under the
    // abstract ISpawnService.
    public sealed class EnemySpawnService : IEnemySpawnService
    {
        public int SpawnCount { get; private set; }

        public void Spawn()
        {
            SpawnCount++;
            Debug.Log($"[Spawn] enemy #{SpawnCount}");
        }

        public void Dispose() { }
    }
}
