using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SM.Net
{
    /// <summary>
    /// Authoritative console variables replicated from server to clients.
    /// </summary>
    public class ConsoleVarManager : NetworkBehaviour
    {
        private readonly SyncDictionary<string, float> _floats = new();
        private readonly SyncDictionary<string, int> _ints = new();

        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("[ConsoleVarManager] Init defaults on server.");
            SetFloat("sv_accelerate", 14f);
            SetFloat("sv_airaccelerate", 80f);
            SetFloat("sv_friction", 6f);
            SetFloat("sv_surf_friction", 0.5f);
            SetFloat("sv_gravity", 30f);
            SetInt("sv_autobhop", 1);
            SetFloat("sv_maxspeed", 7f);
            SetFloat("sv_aircap", 0f);
        }

        [ServerRpc(RequireOwnership = false)] public void SetFloatServerRpc(string k, float v) => SetFloat(k, v);
        [ServerRpc(RequireOwnership = false)] public void SetIntServerRpc(string k, int v) => SetInt(k, v);

        private void SetFloat(string k, float v) { _floats[k] = v; Debug.Log($"[ConVar] {k}={v}"); }
        private void SetInt(string k, int v) { _ints[k] = v; Debug.Log($"[ConVar] {k}={v}"); }

        public float GetFloat(string k, float def = 0f) => _floats.TryGetValue(k, out var v) ? v : def;
        public int GetInt(string k, int def = 0) => _ints.TryGetValue(k, out var v) ? v : def;
    }
}
