// Assets/SM/Scripts/Player/PlayerIdentity.cs
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using Steamworks;

namespace SM.Player
{
    /// <summary>
    /// Syncs player's Steam name and SteamID to everyone (new FishNet SyncVar<T> style).
    /// </summary>
    public class PlayerIdentity : NetworkBehaviour
    {
        // Use SyncVar<T> containers instead of [SyncVar] attribute (obsolete in new FishNet).
        public readonly SyncVar<string> PlayerName = new SyncVar<string>();
        public readonly SyncVar<ulong> SteamId64 = new SyncVar<ulong>();

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
            PlayerName.Value = n;
            SteamId64.Value = sid;
            Debug.Log($"[PlayerIdentity] Server stored identity for {OwnerId}: {n} {sid}");
        }
    }
}