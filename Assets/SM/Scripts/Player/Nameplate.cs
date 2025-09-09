// Assets/SM/Scripts/Player/Nameplate.cs
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
            {
                var n = identity.PlayerName.Value;
                text.text = string.IsNullOrEmpty(n) ? "Player" : n;
            }
            Debug.Log("[Nameplate] Init.");
        }

        private void LateUpdate()
        {
            if (Camera.main == null || follow == null) return;
            transform.position = follow.position + Vector3.up * height;
            transform.forward = Camera.main.transform.forward;

            if (identity != null && text != null)
            {
                var n = identity.PlayerName.Value;
                if (!string.IsNullOrEmpty(n)) text.text = n;
            }
        }
    }
}