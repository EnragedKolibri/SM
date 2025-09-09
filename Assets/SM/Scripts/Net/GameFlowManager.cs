// Assets/SM/Scripts/Net/GameFlowManager.cs
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SM.Net
{
    /// <summary>
    /// Server-authoritative match timer and respawns. Ends match -> back to MainMenu.
    /// </summary>
    public class GameFlowManager : NetworkBehaviour
    {
        [Header("Config")]
        [SerializeField] private int matchSeconds = 180;
        [SerializeField] private float killY = -100f;
        [SerializeField] private string mainMenuScene = "MainMenu";

        [Header("Runtime")]
        private readonly SyncVar<int> _timeLeft = new SyncVar<int>(); // new SyncVar<T> style
        private StartZone _startZone;

        public override void OnStartServer()
        {
            base.OnStartServer();

            // Take matchSeconds from SessionData if present
            matchSeconds = Mathf.Max(1, SM.Net.SessionData.MatchSeconds);

            _timeLeft.Value = matchSeconds;
            Debug.Log($"[GameFlowManager] Server match start {_timeLeft.Value}s.");
            InvokeRepeating(nameof(ServerTickSecond), 1f, 1f);

            // Use new API instead of deprecated FindObjectOfType
            _startZone = UnityEngine.Object.FindFirstObjectByType<StartZone>();
            if (_startZone == null) Debug.LogError("[GameFlowManager] No StartZone in scene!");
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            CancelInvoke(nameof(ServerTickSecond));
        }

        private void ServerTickSecond()
        {
            _timeLeft.Value = Mathf.Max(0, _timeLeft.Value - 1);
            if (_timeLeft.Value == 0)
            {
                Debug.Log("[GameFlowManager] Match ended (timer 0).");
                RpcReturnToMenu();
                Invoke(nameof(ServerShutdownToMenu), 0.5f);
            }
        }

        [ObserversRpc(BufferLast = true)]
        private void RpcReturnToMenu()
        {
            Debug.Log("[GameFlowManager] RPC → Load MainMenu (clients).");
            // Fully qualify to avoid conflict with FishNet's SceneManager type
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
        }

        private void ServerShutdownToMenu()
        {
            Debug.Log("[GameFlowManager] Server stopping and loading menu locally.");
            base.NetworkManager.ServerManager.StopConnection(false);
            base.NetworkManager.ClientManager.StopConnection();
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
        }

        public int GetTimeLeft() => _timeLeft.Value;
        public float KillY => killY;

        [ServerRpc(RequireOwnership = false)]
        public void RequestRespawnServerRpc(FishNet.Object.NetworkObject playerObj)
        {
            if (_startZone == null) return;
            Vector3 pos = _startZone.GetRandomPoint();
            Vector3 fwd = _startZone.Forward();
            Debug.Log($"[GameFlowManager] Respawn {playerObj.OwnerId} at {pos}");
            var motor = playerObj.GetComponent<SM.Player.SurfBhopMotor>();
            if (motor != null) motor.ServerTeleport(pos, fwd);
        }
    }
}