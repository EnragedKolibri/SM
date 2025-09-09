using UnityEngine;
using FishNet.Object;

namespace SM.Player
{
    /// <summary>
    /// Enables local camera for owner.
    /// </summary>
    public class PlayerNetwork : NetworkBehaviour
    {
        [SerializeField] private Camera playerCamera;

        public override void OnStartClient()
        {
            base.OnStartClient();
            bool mine = IsOwner;
            if (playerCamera != null) playerCamera.enabled = mine;
            Debug.Log($"[PlayerNetwork] OnStartClient Owner={mine} NetId={NetworkObject.ObjectId}");
        }
    }
}
