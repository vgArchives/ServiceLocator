namespace Fy.Services.Examples
{
    // The second implementation. Swapping between this and the Normal one is
    // what the [DynamicService] attribute exists for.
    public sealed class HardDifficultyService : IDifficultyService
    {
        public string Name => "Hard";
        public float EnemyHealthMultiplier => 2f;

        public void Dispose() { }
    }
}
