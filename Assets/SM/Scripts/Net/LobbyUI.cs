using UnityEngine;
using UnityEngine.UIElements;
using Steamworks;
using FishNet.Managing;
using System.Collections.Generic;

namespace SM.Net
{
    /// <summary>
    /// Lobby UI: scrollable list with avatars, ready state, Ready/Unready, Force Start (host), Leave.
    /// </summary>
    public class LobbyUI : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private LobbyNetwork lobbyNet;
        [SerializeField] private SteamLobbyManager steamLobbyManager;
        [SerializeField] private NetworkManager fishNet;
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private int defaultMatchSeconds = 180;

        private ScrollView _list;
        private Button _readyBtn;
        private Button _leaveBtn;
        private Button _forceStartBtn;
        private bool _ready;

        private void Awake()
        {
            var root = uiDocument.rootVisualElement;
            root.style.paddingLeft = 12; root.style.paddingTop = 12; root.style.flexDirection = FlexDirection.Column;

            var title = new Label("Lobby — Friends Only");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 16;
            root.Add(title);

            _list = new ScrollView();
            _list.style.height = 320;
            _list.style.marginTop = 8;
            root.Add(_list);

            var rowBtns = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 8 } };
            root.Add(rowBtns);

            _readyBtn = new Button(ToggleReady) { text = "Ready" };
            rowBtns.Add(_readyBtn);

            _forceStartBtn = new Button(ForceStart) { text = "Force Start (Host)" };
            rowBtns.Add(_forceStartBtn);

            _leaveBtn = new Button(LeaveLobby) { text = "Leave Lobby" };
            rowBtns.Add(_leaveBtn);

            Debug.Log("[LobbyUI] Built.");
        }

        private void OnEnable() => InvokeRepeating(nameof(RefreshList), 0.2f, 0.5f);
        private void OnDisable() => CancelInvoke(nameof(RefreshList));

        private void ToggleReady()
        {
            _ready = !_ready;
            _readyBtn.text = _ready ? "Unready" : "Ready";

            int clientId = fishNet.ClientManager.Connection.ClientId;
            string name = SteamFriends.GetPersonaName();
            ulong sid = SteamUser.GetSteamID().m_SteamID;

            Debug.Log($"[LobbyUI] Ready={_ready} for clientId={clientId} name={name}");
            lobbyNet.SetReadyServerRpc(clientId, _ready, name, sid);
        }

        private void ForceStart()
        {
            string sceneName = SM.Net.SessionData.SelectedSceneName;
            int matchSec = SM.Net.SessionData.MatchSeconds;
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[LobbyUI] No SelectedSceneName set. Did Host choose a map on Main Menu?");
                return;
            }

            Debug.Log($"[LobbyUI] ForceStart scene={sceneName} t={matchSec}s");
            lobbyNet.ForceStartServerRpc(sceneName, matchSec);
        }

        private void LeaveLobby()
        {
            Debug.Log("[LobbyUI] Leave clicked. Disconnecting + loading MainMenu.");
            fishNet.ClientManager.StopConnection();
            fishNet.ServerManager.StopConnection(false);
            steamLobbyManager.LeaveLobby();
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
        }

        private class Row
        {
            public VisualElement root;
            public Image avatar;
            public Label name;
            public Label status;
        }

        private readonly Dictionary<int, Row> _rows = new();

        private void RefreshList()
        {
            _forceStartBtn.SetEnabled(fishNet.IsServerStarted);

            var names = lobbyNet.Names;
            var ready = lobbyNet.Ready;
            var steamIds = lobbyNet.SteamIds;

            foreach (var kvp in fishNet.ClientManager.Clients)
            {
                int clientId = kvp.Key;
                if (!_rows.ContainsKey(clientId))
                {
                    var r = new Row();
                    r.root = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 4 } };
                    r.avatar = new Image { scaleMode = ScaleMode.ScaleToFit };
                    r.avatar.style.width = 32; r.avatar.style.height = 32;
                    r.name = new Label($"Client {clientId}");
                    r.status = new Label("…");
                    r.status.style.color = Color.gray;
                    r.root.Add(r.avatar); r.root.Add(r.name); r.root.Add(r.status);
                    _list.Add(r.root);
                    _rows[clientId] = r;
                }

                var row = _rows[clientId];
                string nick = names.ContainsKey(clientId) ? names[clientId] : $"Client {clientId}";
                row.name.text = nick;

                if (steamIds.ContainsKey(clientId))
                {
                    var tex = SM.Steam.SteamAvatarCache.GetSmallAvatar(new CSteamID(steamIds[clientId]));
                    if (tex != null) row.avatar.image = tex;
                }

                bool isReady = ready.ContainsKey(clientId) && ready[clientId];
                row.status.text = isReady ? "Ready" : "Not Ready";
                row.status.style.color = isReady ? new StyleColor(new Color(0.3f, 1f, 0.3f)) : new StyleColor(Color.gray);
            }
        }
    }
}
