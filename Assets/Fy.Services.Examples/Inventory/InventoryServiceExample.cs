using Fy.Services;
using UnityEngine;

namespace Fy.Services.Examples
{
    // Drop this on any GameObject and press Play. Watch the Console.
    //
    // This shows the everyday case: ask for a service, use it. No setup, no
    // singletons to wire up, no scene object to drag into a field.
    public sealed class InventoryServiceExample : MonoBehaviour
    {
        private void Start()
        {
            // TryGet returns false if nothing can provide the service. For an
            // auto-registered service like this one it will always succeed, but
            // checking the result is the habit worth keeping.
            if (!ServiceLocator.TryGet(out IInventoryService inventory))
            {
                Debug.LogWarning("Inventory service was not available.");
                return;
            }

            inventory.Add("Potion", 3);
            inventory.Remove("Potion");

            Debug.Log($"Potions left: {inventory.Count("Potion")}");
        }
    }
}
