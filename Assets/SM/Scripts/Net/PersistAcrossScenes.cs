using UnityEngine;

namespace SM.Net
{
    /// <summary>
    /// Tag any GameObject with this to persist across scene loads.
    /// </summary>
    public class PersistAcrossScenes : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[PersistAcrossScenes] '{name}' marked DontDestroyOnLoad.");
        }
    }
}
