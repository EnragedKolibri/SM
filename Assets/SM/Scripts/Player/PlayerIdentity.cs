using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using Steamworks;

namespace SM.Player
{
    /// <summary>
    /// Syncs player's Steam name and SteamID to everyone.
    /// </summary>
    public class PlayerIdentity : NetworkBehaviour
    {
        [SyncVar] public string PlayerName;
        [SyncVar] public ulong SteamId64;

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwner)
            {
                string name = SteamFriends.GetPersonaName();
                ulong sid = SteamUser.GetSteamID().m_SteamID;
                SubmitIdentityServerRpc(name, sid);
                Debug.Log($"[PlayerIdentity] Submitted identity {name} {sid}");
            }
        }

        [ServerRpc]
        private void SubmitIdentityServerRpc(string n, ulong sid)
        {
            PlayerName = n;
            SteamId64 = sid;
            Debug.Log($"[PlayerIdentity] Server stored identity for {OwnerId}: {n} {sid}");
        }
    }
}
