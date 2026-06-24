using Fy.Services;
using UnityEngine;

namespace Fy.Services.Examples
{
    // Drop this on a GameObject and press Play.
    //
    // From the caller's side a preloaded service is resolved like any other.
    // The only observable difference is timing: the "[Localization] preloaded"
    // line in the Console prints during load, before this Start() runs, because
    // the service was built ahead of time rather than on this TryGet.
    public sealed class LocalizationServiceExample : MonoBehaviour
    {
        private void Start()
        {
            if (!ServiceLocator.TryGet(out ILocalizationService localization))
            {
                Debug.LogWarning("Localization service was not available.");
                return;
            }

            Debug.Log($"hello -> \"{localization.Get("hello")}\"");
        }
    }
}
