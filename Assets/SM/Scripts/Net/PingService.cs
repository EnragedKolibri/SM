using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using Steamworks;

namespace SM.Net
{
    /// <summary>
    /// Simple RTT tracking + name/steam id registry synced to everyone.
    /// </summary>
    public class PingService : NetworkBehaviour
    {
        private class LocalPing { public double sentTime; public bool pending; }

        private readonly Dictionary<int, LocalPing> _locals = new();
        private float _nextPing;

        private readonly SyncDictionary<int, int> _pings = new();
        private readonly SyncDictionary<int, string> _names = new();
        private readonly SyncDictionary<int, ulong> _steamIds = new();

        private void Update()
        {
            if (!IsClient) return;

            if (Time.time >= _nextPing)
            {
                _nextPing = Time.time + 2f;
                SendPing();
            }
        }

        private void SendPing()
        {
            int cid = base.NetworkManager.ClientManager.Connection.ClientId;
            double now = Time.unscaledTimeAsDouble;
            _locals[cid] = new LocalPing { sentTime = now, pending = true };
            string name = SteamFriends.GetPersonaName();
            ulong sid = SteamUser.GetSteamID().m_SteamID;

            Debug.Log($"[PingService] SendPing cid={cid}");
            PingServerRpc(cid, now, name, sid);
        }

        [ServerRpc(RequireOwnership = false)]
        private void PingServerRpc(int cid, double sentClientTime, string name, ulong sid)
        {
            PongTargetRpc(Owner, cid, sentClientTime);
            _names[cid] = name; _steamIds[cid] = sid;
        }

        [TargetRpc]
        private void PongTargetRpc(NetworkConnection conn, int cid, double sentClientTime)
        {
            if (!_locals.TryGetValue(cid, out var rec) || !rec.pending) return;
            rec.pending = false;
            double rtt = (Time.unscaledTimeAsDouble - rec.sentTime) * 1000.0;
            int ms = Mathf.Clamp((int)System.Math.Round(rtt), 0, 999);
            Debug.Log($"[PingService] RTT {ms} ms. Reporting.");
            ReportRttServerRpc(cid, ms);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ReportRttServerRpc(int cid, int rttMs)
        {
            _pings[cid] = rttMs;
        }

        public int GetPing(int cid) => _pings.TryGetValue(cid, out int p) ? p : -1;
        public string GetName(int cid) => _names.TryGetValue(cid, out var n) ? n : $"Client {cid}";
        public ulong GetSteamId(int cid) => _steamIds.TryGetValue(cid, out var s) ? s : 0;
    }
}
