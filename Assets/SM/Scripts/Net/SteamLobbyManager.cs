using UnityEngine;
using Steamworks;
using FishNet.Managing;
using FishySteamworks;
using System;

namespace SM.Net
{
    /// <summary>
    /// Handles Steam lobby creation/join and FishNet P2P connection (FishySteamworks).
    /// </summary>
    public class SteamLobbyManager : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private FishySteamworks fishyTransport;

        [Header("State (read-only)")]
        [SerializeField] private ulong currentLobbyId;

        // Lobby metadata keys
        private const string KEY_HOST  = "host_steamid";
        private const string KEY_STATE = "state";       // lobby | in_game
        private const string KEY_MAP   = "map";
        private const string KEY_MAX   = "max";

        private Callback<LobbyCreated_t> _cbCreated;
        private Callback<LobbyEnter_t> _cbEntered;
        private Callback<GameLobbyJoinRequested_t> _cbJoinReq;

        public event Action OnLobbyEntered;
        public event Action OnLobbyLeft;

        private bool _hosting;
        private int _pendingMax;
        private string _pendingMap;

        public bool IsInLobby => currentLobbyId != 0;
        public bool IsHost => _hosting;

        private void Awake()
        {
            _cbCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            _cbEntered = Callback<LobbyEnter_t>.Create(OnLobbyEnteredCb);
            _cbJoinReq = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
            Debug.Log("[SteamLobbyManager] Callbacks registered.");
        }

        public void CreateLobby(int maxPlayers, string selectedSceneName)
        {
            _hosting = true;
            _pendingMax = Mathf.Clamp(maxPlayers, 2, 10);
            _pendingMap = selectedSceneName ?? string.Empty;
            Debug.Log($"[SteamLobbyManager] Creating FriendsOnly lobby. max={_pendingMax} map={_pendingMap}");
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, _pendingMax);
        }

        public void LeaveLobby()
        {
            if (!IsInLobby) return;
            Debug.Log("[SteamLobbyManager] Leaving lobby.");
            SteamMatchmaking.LeaveLobby(new CSteamID(currentLobbyId));
            currentLobbyId = 0;
            _hosting = false;
            OnLobbyLeft?.Invoke();
        }

        private void OnLobbyCreated(LobbyCreated_t e)
        {
            if (e.m_eResult != EResult.k_EResultOK)
            {
                Debug.LogError($"[SteamLobbyManager] Create failed: {e.m_eResult}");
                _hosting = false;
                return;
            }

            currentLobbyId = e.m_ulSteamIDLobby;
            var lobby = new CSteamID(currentLobbyId);
            var me = SteamUser.GetSteamID();

            SteamMatchmaking.SetLobbyData(lobby, KEY_HOST, me.m_SteamID.ToString());
            SteamMatchmaking.SetLobbyData(lobby, KEY_STATE, "lobby");
            SteamMatchmaking.SetLobbyData(lobby, KEY_MAP, _pendingMap);
            SteamMatchmaking.SetLobbyData(lobby, KEY_MAX, _pendingMax.ToString());
            SteamMatchmaking.SetLobbyJoinable(lobby, true);

            Debug.Log($"[SteamLobbyManager] Lobby created {currentLobbyId}, host={me.m_SteamID}");

            // Start listen host networking.
            networkManager.ServerManager.StartConnection();
            networkManager.ClientManager.StartConnection();
        }

        private void OnLobbyEnteredCb(LobbyEnter_t e)
        {
            currentLobbyId = e.m_ulSteamIDLobby;
            Debug.Log($"[SteamLobbyManager] Entered lobby {currentLobbyId}. Hosting={_hosting}");
            OnLobbyEntered?.Invoke();

            if (!_hosting)
            {
                var lobby = new CSteamID(currentLobbyId);
                string hostIdStr = SteamMatchmaking.GetLobbyData(lobby, KEY_HOST);
                if (ulong.TryParse(hostIdStr, out ulong hostId))
                {
                    fishyTransport.SetClientAddress(hostId);
                    Debug.Log($"[SteamLobbyManager] Connecting to host {hostId}...");
                    networkManager.ClientManager.StartConnection();
                }
                else
                {
                    Debug.LogError("[SteamLobbyManager] Missing host SteamID in lobby data.");
                }
            }
        }

        private void OnJoinRequested(GameLobbyJoinRequested_t e)
        {
            Debug.Log($"[SteamLobbyManager] Join requested via overlay. Lobby={e.m_steamIDLobby.m_SteamID}");
            SteamMatchmaking.JoinLobby(e.m_steamIDLobby);
        }

        public void SetLobbyInGame(string sceneName)
        {
            if (!IsInLobby) return;
            var lobby = new CSteamID(currentLobbyId);
            SteamMatchmaking.SetLobbyData(lobby, KEY_STATE, "in_game");
            SteamMatchmaking.SetLobbyData(lobby, KEY_MAP, sceneName);
            SteamMatchmaking.SetLobbyJoinable(lobby, false);
            Debug.Log("[SteamLobbyManager] Lobby set to in_game; late joiners blocked.");
        }
    }
}
