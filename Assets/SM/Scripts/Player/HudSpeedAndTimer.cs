using UnityEngine;
using UnityEngine.UIElements;

namespace SM.Player
{
    /// <summary>
    /// Simple HUD labels for speed and match time.
    /// </summary>
    public class HudSpeedAndTimer : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private SurfBhopMotor motor;
        private Label _speed, _timer;
        private SM.Net.GameFlowManager _flow;

        private void Start()
        {
            _flow = FindObjectOfType<SM.Net.GameFlowManager>();
            var root = uiDocument.rootVisualElement;
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, gap = 12 } };
            _speed = new Label("Speed: 0");
            _timer = new Label("Time: 0");
            row.Add(_speed); row.Add(_timer);
            root.Add(row);
            Debug.Log("[HudSpeedAndTimer] HUD built.");
        }

        private void Update()
        {
            if (motor != null)
                _speed.text = $"Speed: {motor.LateralSpeed:F1}";

            if (_flow != null)
                _timer.text = $"Time: {_flow.GetTimeLeft()}";
        }
    }
}
