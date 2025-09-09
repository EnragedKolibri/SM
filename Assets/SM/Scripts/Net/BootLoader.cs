using UnityEngine;
using UnityEngine.SceneManagement;

namespace SM.Net
{
    /// <summary>
    /// Simple boot: waits one frame to ensure Steam/Net are up, then loads MainMenu.
    /// Place this in the 00_Boot scene on an empty GameObject.
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        [SerializeField] private string mainMenuScene = "MainMenu";

        private void Start()
        {
            Debug.Log("[BootLoader] Loading MainMenu...");
            // Load MainMenu after a very short delay to allow Awake() of bootstrap components.
            Invoke(nameof(LoadMenu), 0.1f);
        }

        private void LoadMenu()
        {
            SceneManager.LoadScene(mainMenuScene);
        }
    }
}
