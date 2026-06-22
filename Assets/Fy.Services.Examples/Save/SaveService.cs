using System.Collections.Generic;
using UnityEngine;

namespace Fy.Services.Examples
{
    // An ordinary service again. Being [RequiredService] changes how callers
    // ask for it, not how you write it.
    public sealed class SaveService : ISaveService
    {
        private readonly Dictionary<string, string> _slots = new();

        public void Save(string slot)
        {
            _slots[slot] = $"saved@{Time.realtimeSinceStartup:0.00}";
            Debug.Log($"[Save] wrote slot '{slot}'");
        }

        public bool TryLoad(string slot, out string data)
        {
            return _slots.TryGetValue(slot, out data);
        }

        public void Dispose()
        {
            _slots.Clear();
        }
    }
}
