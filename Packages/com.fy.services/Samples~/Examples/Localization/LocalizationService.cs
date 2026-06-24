using System.Collections.Generic;
using UnityEngine;

namespace Fy.Services.Examples
{
    // The [PreloadService] attribute goes on the implementation. The auto-loader
    // creates this instance before the first scene loads, so by the time your
    // game code runs the strings are already in memory.
    //
    // The log line in the constructor is just so you can see in the Console
    // *when* it happens: it prints during load, before any of the example
    // components' Start() runs.
    [PreloadService]
    public sealed class LocalizationService : ILocalizationService
    {
        private readonly Dictionary<string, string> _strings = new()
        {
            ["hello"] = "Hello!",
            ["quit"] = "Quit Game",
        };

        public LocalizationService()
        {
            Debug.Log($"[Localization] preloaded {_strings.Count} strings");
        }

        public string Get(string key)
        {
            return _strings.GetValueOrDefault(key, key);
        }

        public void Dispose()
        {
            _strings.Clear();
        }
    }
}
