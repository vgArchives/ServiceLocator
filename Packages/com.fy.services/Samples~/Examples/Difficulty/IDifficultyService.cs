using Fy.Services;

namespace Fy.Services.Examples
{
    // [DynamicService] marks a service you intend to swap at runtime.
    //
    // By default the locator refuses to overwrite a service that's already set
    // and logs an error, which catches accidental double-registration. When the
    // interface is marked [DynamicService] that guard is lifted: calling
    // SetService again replaces the current instance and disposes the old one.
    //
    // Difficulty is a good fit because the player picks it from a menu and it
    // can change between runs.
    [DynamicService]
    public interface IDifficultyService : IService
    {
        string Name { get; }
        float EnemyHealthMultiplier { get; }
    }
}
