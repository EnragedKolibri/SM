using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace SM.Net
{
    /// <summary>
    /// Main menu built in code (uses UIDocument). Host lobby with MaxPlayers + Map dropdown.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private SteamLobbyManager lobbyManager;
        [SerializeField] private MapCatalog mapCatalog;
        [SerializeField] private string lobbySceneName = "Lobby";

        private IntegerField _maxPlayersField;
        private DropdownField _mapDropdown;

        private void Awake()
        {
            var root = uiDocument.rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;
            root.style.paddingLeft = 16; root.style.paddingTop = 16; root.style.paddingRight = 16;

            var title = new Label("SM Surf/Bhop — Main Menu");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 18;
            root.Add(title);

            _maxPlayersField = new IntegerField("Max Players") { value = 10 };
            _maxPlayersField.style.marginTop = 8;
            root.Add(_maxPlayersField);

            _mapDropdown = new DropdownField("Select Map");
            _mapDropdown.style.marginTop = 8;
            if (mapCatalog != null && mapCatalog.Maps != null)
            {
                foreach (var m in mapCatalog.Maps) _mapDropdown.choices.Add(m.DisplayName);
                if (_mapDropdown.choices.Count > 0) _mapDropdown.value = _mapDropdown.choices[0];
            }
            root.Add(_mapDropdown);

            var hostBtn = new Button(OnHostClicked) { text = "Host Lobby" };
            hostBtn.style.marginTop = 12;
            root.Add(hostBtn);

            var quitBtn = new Button(() => Application.Quit()) { text = "Quit" };
            quitBtn.style.marginTop = 4;
            root.Add(quitBtn);

            Debug.Log("[MainMenuUI] Built main menu UI.");
        }

        private void OnEnable()
        {
            if (lobbyManager != null)
                lobbyManager.OnLobbyEntered += OnLobbyEntered;
        }

        private void OnDisable()
        {
            if (lobbyManager != null)
                lobbyManager.OnLobbyEntered -= OnLobbyEntered;
        }

        private void OnHostClicked()
        {
            int maxP = Mathf.Clamp(_maxPlayersField.value, 2, 10);
            int idx = Mathf.Max(0, _mapDropdown.index);
            string sceneMeta = GetSelectedSceneName();
            // NEW: stash selection for lobby/gameplay use.
            SM.Net.SessionData.SelectedSceneName = sceneMeta;
            if (mapCatalog != null && mapCatalog.Maps != null && idx < mapCatalog.Maps.Length)
                SM.Net.SessionData.MatchSeconds = Mathf.Max(30, mapCatalog.Maps[idx].DefaultMatchSeconds); // guard minimum
            else
                SM.Net.SessionData.MatchSeconds = 180;

            Debug.Log($"[MainMenuUI] Host clicked. max={maxP} map={sceneMeta} time={SM.Net.SessionData.MatchSeconds}s");
            lobbyManager.CreateLobby(maxP, sceneMeta);
        }

        private string GetSelectedSceneName()
        {
            if (mapCatalog == null || mapCatalog.Maps == null || mapCatalog.Maps.Length == 0)
                return string.Empty;
            int idx = Mathf.Max(0, _mapDropdown.index);
            return mapCatalog.Maps[idx].SceneName;
        }

        private void OnLobbyEntered()
        {
            Debug.Log("[MainMenuUI] Lobby entered. Loading Lobby scene...");
            SceneManager.LoadScene(lobbySceneName);
        }
    }
}
