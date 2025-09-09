using UnityEngine;
using UnityEngine.UIElements;
using FishNet.Managing;

namespace SM.Net
{
    /// <summary>
    /// Backquote (`) console, host-only. Supports sv_* vars defined in ConsoleVarManager.
    /// </summary>
    public class ConsoleUI : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private ConsoleVarManager convars;
        [SerializeField] private NetworkManager fishNet;

        private VisualElement _panel;
        private TextField _input;
        private Label _log;

        private void Awake()
        {
            var root = uiDocument.rootVisualElement;
            _panel = new VisualElement();
            _panel.style.position = Position.Absolute;
            _panel.style.left = 20; _panel.style.top = 20; _panel.style.width = 420;
            _panel.style.backgroundColor = new Color(0,0,0,0.8f);
            _panel.style.paddingLeft = 6; _panel.style.paddingTop = 6; _panel.style.paddingRight = 6; _panel.style.paddingBottom = 6;
            _panel.style.display = DisplayStyle.None;

            _log = new Label("Console (host only)");
            _log.style.whiteSpace = WhiteSpace.Normal;
            _log.style.height = 120;
            _panel.Add(_log);

            _input = new TextField(">");
            _input.RegisterCallback<KeyDownEvent>(OnKeyDown);
            _panel.Add(_input);

            root.Add(_panel);
            Debug.Log("[ConsoleUI] Built.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
                _panel.style.display = _panel.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;

            _panel.SetEnabled(fishNet.IsServerStarted); // host only
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Return) return;
            string cmd = _input.value.Trim();
            if (string.IsNullOrEmpty(cmd)) return;
            _input.value = "";
            Execute(cmd);
        }

        private void Execute(string cmd)
        {
            Log($"> {cmd}");
            var parts = cmd.Split(' ');
            if (parts.Length == 0) return;
            string c = parts[0].ToLowerInvariant();

            bool TryF(out float v)
            {
                v = 0f; if (parts.Length < 2) { Log("Value missing."); return false; }
                if (!float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v)) { Log("Bad number."); return false; }
                return true;
            }
            bool TryI(out int v)
            {
                v = 0; if (parts.Length < 2) { Log("Value missing."); return false; }
                if (!int.TryParse(parts[1], out v)) { Log("Bad int."); return false; }
                return true;
            }

            switch (c)
            {
                case "sv_accelerate": if (TryF(out var f1)) convars.SetFloatServerRpc(c, f1); break;
                case "sv_airaccelerate": if (TryF(out var f2)) convars.SetFloatServerRpc(c, f2); break;
                case "sv_friction": if (TryF(out var f3)) convars.SetFloatServerRpc(c, f3); break;
                case "sv_surf_friction": if (TryF(out var f4)) convars.SetFloatServerRpc(c, f4); break;
                case "sv_gravity": if (TryF(out var f5)) convars.SetFloatServerRpc(c, f5); break;
                case "sv_autobhop": if (TryI(out var i1)) convars.SetIntServerRpc(c, i1); break;
                case "sv_maxspeed": if (TryF(out var f6)) convars.SetFloatServerRpc(c, f6); break;
                case "sv_aircap": if (TryF(out var f7)) convars.SetFloatServerRpc(c, f7); break;
                default: Log("Unknown command."); break;
            }
        }

        private void Log(string line) => _log.text += "\n" + line;
    }
}
