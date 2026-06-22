using Fy.Services;
using UnityEngine;

namespace Fy.Services.Examples
{
    // Drop this on a GameObject and press Play.
    //
    // For something the game truly depends on, GetChecked reads cleaner than
    // TryGet: no "if it failed" branch to write, because a failure throws and
    // surfaces the problem loudly instead of returning a quiet null.
    public sealed class SaveServiceExample : MonoBehaviour
    {
        private void Start()
        {
            ISaveService save = ServiceLocator.GetChecked<ISaveService>();

            save.Save("slot1");
            bool loaded = save.TryLoad("slot1", out string data);

            Debug.Log($"Loaded slot1: {loaded} ({data})");
        }
    }
}
