using UnityEngine;

namespace LudumDare58
{
    // Deprecated shim: drift is built into Vehicle now.
    // Keep this only to avoid “Missing Script” in old scenes/prefabs.
    [AddComponentMenu("")] // hides from Add Component menu
    public class VehiculeDrift : MonoBehaviour
    {
        void OnEnable()
        {
            Debug.LogWarning("VehiculeDrift is deprecated. Remove this component and use Vehicle (drift is built-in).");
            enabled = false;
        }
    }
}
