using UnityEngine;
using Steamworks;

namespace SM.Steam
{
    /// <summary>
    /// Boots Steamworks.NET once and persists across scenes.
    /// Put this in the 00_Boot scene.
    /// </summary>
    public class SteamBootstrap : MonoBehaviour
    {
        private static SteamBootstrap _instance;

        private void Awake()
        {
            // Ensure a single instance (singleton-ish) and persist across scenes.
            if (_instance != null)
            {
                Debug.LogWarning("[SteamBootstrap] Duplicate detected; destroying this instance.");
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Try to initialize Steam — requires Steam client running and steam_appid.txt (480 for dev) beside the exe/editor.
            try
            {
                if (!SteamAPI.Init())
                {
                    Debug.LogError("[SteamBootstrap] SteamAPI.Init failed. Is Steam running? Is steam_appid.txt present with 480?");
                }
                else
                {
                    Debug.Log($"[SteamBootstrap] Steam initialized as {SteamFriends.GetPersonaName()} ({SteamUser.GetSteamID().m_SteamID}).");
                }
            }
            catch (System.DllNotFoundException e)
            {
                Debug.LogError($"[SteamBootstrap] Missing Steam native DLLs: {e.Message}");
            }
        }

        private void Update()
        {
            // Pump callbacks every frame.
            SteamAPI.RunCallbacks();
        }

        private void OnApplicationQuit()
        {
            Debug.Log("[SteamBootstrap] Shutdown Steam API.");
            SteamAPI.Shutdown();
        }
    }
}
