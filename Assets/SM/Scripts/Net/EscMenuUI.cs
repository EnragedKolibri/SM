using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using FishNet.Managing;

namespace SM.Net
{
    /// <summary>
    /// Simple ESC overlay (non-pausing). Exit -> MainMenu.
    /// </summary>
    public class EscMenuUI : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private NetworkManager fishNet;

        private VisualElement _panel;

        private void Awake()
        {
            var root = uiDocument.rootVisualElement;
            _panel = new VisualElement();
            _panel.style.position = Position.Absolute;
            _panel.style.left = 10; _panel.style.top = 10;
            _panel.style.backgroundColor = new Color(0,0,0,0.6f);
            _panel.style.paddingLeft = 6; _panel.style.paddingTop = 6; _panel.style.paddingRight = 6; _panel.style.paddingBottom = 6;
            _panel.style.display = DisplayStyle.None;

            var btn = new Button(ExitToMenu) { text = "Exit to Main Menu" };
            _panel.Add(btn);
            root.Add(_panel);

            Debug.Log("[EscMenuUI] Built.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                _panel.style.display = _panel.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void ExitToMenu()
        {
            Debug.Log("[EscMenuUI] Exit to Main Menu requested.");
            fishNet.ClientManager.StopConnection();
            fishNet.ServerManager.StopConnection(false);
            SceneManager.LoadScene(mainMenuScene);
        }
    }
}
