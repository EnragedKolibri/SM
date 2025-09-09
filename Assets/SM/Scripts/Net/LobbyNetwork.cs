using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Managing.Scened;
using UnityEngine;

namespace SM.Net
{
    /// <summary>
    /// Minimal lobby state replicated by server. Ready flags, names, steam ids.
    /// </summary>
    public class LobbyNetwork : NetworkBehaviour
    {
        [Header("Runtime")]
        [SerializeField] private string selectedSceneName = "";

        private readonly SyncDictionary<int, bool> _ready = new();
        private readonly SyncDictionary<int, string> _names = new();
        private readonly SyncDictionary<int, ulong> _steamIds = new();

        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("[LobbyNetwork] Server start; clearing state.");
            _ready.Clear(); _names.Clear(); _steamIds.Clear();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            Debug.Log($"[LobbyNetwork] Client start. IsServer={IsServer}");
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetReadyServerRpc(int clientId, bool ready, string steamName, ulong steamId)
        {
            _ready[clientId] = ready;
            _names[clientId] = steamName;
            _steamIds[clientId] = steamId;
            Debug.Log($"[LobbyNetwork] Ready[{clientId}]={ready} {steamName}");
        }

        public IReadOnlyDictionary<int, bool> Ready => _ready;
        public IReadOnlyDictionary<int, string> Names => _names;
        public IReadOnlyDictionary<int, ulong> SteamIds => _steamIds;

        [ServerRpc(RequireOwnership = false)]
        public void ForceStartServerRpc(string sceneName, int matchSeconds)
        {
            SM.Net.SessionData.SelectedSceneName = sceneName;
            SM.Net.SessionData.MatchSeconds = matchSeconds;

            Debug.Log($"[LobbyNetwork] ForceStart scene={sceneName} t={matchSeconds}s");
            selectedSceneName = sceneName;

            var sld = new SceneLoadData(sceneName) { ReplaceScenes = ReplaceOption.All };
            NetworkManager.SceneManager.LoadGlobalScenes(sld);

            var steamLobby = FindObjectOfType<SteamLobbyManager>();
            if (steamLobby != null)
                steamLobby.SetLobbyInGame(sceneName);
        }
    }
}
