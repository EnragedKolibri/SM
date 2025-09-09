using UnityEngine;
using UnityEngine.UIElements;
using FishNet.Managing;
using Steamworks;

namespace SM.Net
{
    /// <summary>
    /// In-game TAB overlay with nick, ping, and Steam avatar.
    /// </summary>
    public class TabScoreboardUI : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private NetworkManager fishNet;
        [SerializeField] private PingService pingService;

        private VisualElement _panel;
        private ScrollView _list;

        private void Awake()
        {
            var root = uiDocument.rootVisualElement;
            _panel = new VisualElement();
            _panel.style.position = Position.Absolute;
            _panel.style.left = 10; _panel.style.top = 60;
            _panel.style.width = 320; _panel.style.maxHeight = 400;
            _panel.style.backgroundColor = new Color(0,0,0,0.6f);
            _panel.style.paddingLeft = 6; _panel.style.paddingTop = 6; _panel.style.paddingRight = 6; _panel.style.paddingBottom = 6;
            _panel.style.display = DisplayStyle.None;

            var title = new Label("Players");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            _panel.Add(title);

            _list = new ScrollView();
            _panel.Add(_list);

            root.Add(_panel);
            Debug.Log("[TabScoreboardUI] Built.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab)) _panel.style.display = DisplayStyle.Flex;
            if (Input.GetKeyUp(KeyCode.Tab)) _panel.style.display = DisplayStyle.None;

            if (_panel.style.display == DisplayStyle.Flex)
                Refresh();
        }

        private void Refresh()
        {
            _list.Clear();
            foreach (var kv in fishNet.ClientManager.Clients)
            {
                int cid = kv.Key;
                string name = pingService.GetName(cid);
                int ping = pingService.GetPing(cid);
                ulong sid = pingService.GetSteamId(cid);

                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, gap = 8, marginBottom = 4 } };
                var img = new Image { style = { width = 24, height = 24 } };
                if (sid != 0)
                {
                    var tex = SM.Steam.SteamAvatarCache.GetSmallAvatar(new CSteamID(sid));
                    if (tex != null) img.image = tex;
                }
                row.Add(img);
                row.Add(new Label($"{name}"));
                row.Add(new Label($"{ping} ms"));
                _list.Add(row);
            }
        }
    }
}
