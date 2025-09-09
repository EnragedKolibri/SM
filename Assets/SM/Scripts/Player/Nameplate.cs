using UnityEngine;
using TMPro;

namespace SM.Player
{
    /// <summary>
    /// World-space label above head. Binds to PlayerIdentity for correct remote names.
    /// </summary>
    public class Nameplate : MonoBehaviour
    {
        [SerializeField] private TextMeshPro text;
        [SerializeField] private Transform follow;
        [SerializeField] private float height = 2.0f;
        [SerializeField] private PlayerIdentity identity;

        private void Start()
        {
            if (identity != null && text != null)
                text.text = string.IsNullOrEmpty(identity.PlayerName) ? "Player" : identity.PlayerName;
            Debug.Log("[Nameplate] Init.");
        }

        private void LateUpdate()
        {
            if (Camera.main == null || follow == null) return;
            transform.position = follow.position + Vector3.up * height;
            transform.forward = Camera.main.transform.forward;
            if (identity != null && text != null && !string.IsNullOrEmpty(identity.PlayerName))
                text.text = identity.PlayerName;
        }
    }
}
