// Assets/SM/Scripts/Player/HudSpeedAndTimer.cs
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
            // Use new API instead of deprecated FindObjectOfType
            _flow = UnityEngine.Object.FindFirstObjectByType<SM.Net.GameFlowManager>();

            var root = uiDocument.rootVisualElement;
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;

            _speed = new Label("Speed: 0");
            _speed.style.marginRight = new Length(12); // spacing (no 'gap' in your runtime)

            _timer = new Label("Time: 0");

            row.Add(_speed);
            row.Add(_timer);
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