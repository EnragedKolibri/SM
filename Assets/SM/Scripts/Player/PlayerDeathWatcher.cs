using UnityEngine;
using FishNet.Object;
using UnityEngine.UIElements;

namespace SM.Player
{
    /// <summary>
    /// Triggers death when below KillY, shows 5s overlay, requests respawn.
    /// </summary>
    public class PlayerDeathWatcher : NetworkBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        private Label _text;
        private VisualElement _panel;
        private float _respawnAt = -1f;
        private SM.Net.GameFlowManager _flow;

        private void Start()
        {
            _flow = FindObjectOfType<SM.Net.GameFlowManager>();
            var root = uiDocument.rootVisualElement;
            _panel = new VisualElement();
            _panel.style.backgroundColor = new Color(0,0,0,0.6f);
            _panel.style.position = Position.Absolute;
            _panel.style.left = 0; _panel.style.right = 0; _panel.style.top = 0; _panel.style.bottom = 0;
            _panel.style.display = DisplayStyle.None;

            _text = new Label("You died. Respawning in 5...");
            _text.style.unityTextAlign = TextAnchor.MiddleCenter;
            _text.style.fontSize = 24;
            _text.style.color = Color.white;
            _text.style.position = Position.Absolute;
            _text.style.left = 0; _text.style.right = 0; _text.style.top = new Length(40, LengthUnit.Percent);
            _panel.Add(_text);
            root.Add(_panel);

            Debug.Log("[PlayerDeathWatcher] UI built.");
        }

        private void Update()
        {
            if (!IsOwner) return;
            if (_flow == null) return;

            if (transform.position.y < _flow.KillY && _respawnAt < 0f)
            {
                _respawnAt = Time.time + 5f;
                _panel.style.display = DisplayStyle.Flex;
                Debug.Log("[PlayerDeathWatcher] Death detected, 5s countdown.");
            }

            if (_respawnAt > 0f)
            {
                float remain = Mathf.Max(0f, _respawnAt - Time.time);
                _text.text = $"You died. Respawning in {Mathf.CeilToInt(remain)}...";
                if (remain <= 0f)
                {
                    _respawnAt = -1f;
                    _panel.style.display = DisplayStyle.None;
                    var no = GetComponent<NetworkObject>();
                    FindObjectOfType<SM.Net.GameFlowManager>().RequestRespawnServerRpc(no);
                }
            }
        }
    }
}
