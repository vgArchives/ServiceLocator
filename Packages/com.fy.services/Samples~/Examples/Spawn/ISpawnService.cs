using Fy.Services;

namespace Fy.Services.Examples
{
    // [AbstractService] marks a base interface that should NOT be resolvable on
    // its own. It's a shared contract for a family of services, not something
    // you fetch directly.
    //
    // The auto-loader skips abstract service interfaces when it hands out
    // factories, so ServiceLocator.TryGet<ISpawnService>() returns nothing.
    // You ask for the concrete interface instead (see IEnemySpawnService).
    [AbstractService]
    public interface ISpawnService : IService
    {
        int SpawnCount { get; }
        void Spawn();
    }
}
