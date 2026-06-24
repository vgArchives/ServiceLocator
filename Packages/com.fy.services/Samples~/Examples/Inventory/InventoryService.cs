using System.Collections.Generic;

namespace Fy.Services.Examples
{
    // Nothing special here. It's a normal class that happens to implement a
    // service interface. Dispose() is called for you when the service is
    // replaced or when the locator is reset, so use it to release anything
    // you allocated.
    public sealed class InventoryService : IInventoryService
    {
        private readonly Dictionary<string, int> _items = new();

        public IReadOnlyDictionary<string, int> Items => _items;

        public void Add(string item, int amount = 1)
        {
            _items.TryGetValue(item, out int current);
            _items[item] = current + amount;
        }

        public bool Remove(string item, int amount = 1)
        {
            if (!_items.TryGetValue(item, out int current) || current < amount)
            {
                return false;
            }

            current -= amount;

            if (current == 0)
            {
                _items.Remove(item);
            }
            else
            {
                _items[item] = current;
            }

            return true;
        }

        public int Count(string item)
        {
            return _items.GetValueOrDefault(item);
        }

        public void Dispose()
        {
            _items.Clear();
        }
    }
}
