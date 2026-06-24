namespace Fy.Services.Examples
{
    // One of two interchangeable implementations of IDifficultyService.
    public sealed class NormalDifficultyService : IDifficultyService
    {
        public string Name => "Normal";
        public float EnemyHealthMultiplier => 1f;

        public void Dispose() { }
    }
}
