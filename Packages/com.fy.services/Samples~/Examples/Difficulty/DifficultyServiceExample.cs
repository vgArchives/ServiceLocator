using Fy.Services;
using UnityEngine;

namespace Fy.Services.Examples
{
    // Drop this on a GameObject and press Play.
    //
    // Here we register the service ourselves with SetService, because which
    // implementation to use is a runtime decision (think: read from a settings
    // menu). Then we swap it for a different one to show the dynamic replace.
    public sealed class DifficultyServiceExample : MonoBehaviour
    {
        private void Start()
        {
            // Choose the starting difficulty explicitly.
            ServiceLocator.SetService<IDifficultyService>(new NormalDifficultyService());
            ServiceLocator.TryGet(out IDifficultyService current);
            Debug.Log($"Difficulty is {current.Name}, enemy health x{current.EnemyHealthMultiplier}");

            // Later, the player bumps it up. Because IDifficultyService is
            // [DynamicService], this replaces the previous instance and
            // disposes it. Without that attribute the locator would log an
            // error and keep the old one.
            ServiceLocator.SetService<IDifficultyService>(new HardDifficultyService());
            ServiceLocator.TryGet(out current);
            Debug.Log($"Difficulty is now {current.Name}, enemy health x{current.EnemyHealthMultiplier}");
        }
    }
}
