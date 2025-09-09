using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        [SyncVar] private int _timeLeft;
        private StartZone _startZone;

        public override void OnStartServer()
        {
            base.OnStartServer();
            _timeLeft = matchSeconds;
            Debug.Log($"[GameFlowManager] Server match start {_timeLeft}s.");
            InvokeRepeating(nameof(ServerTickSecond), 1f, 1f);

            _startZone = FindObjectOfType<StartZone>();
            if (_startZone == null) Debug.LogError("[GameFlowManager] No StartZone in scene!");
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            CancelInvoke(nameof(ServerTickSecond));
        }

        private void ServerTickSecond()
        {
            _timeLeft = Mathf.Max(0, _timeLeft - 1);
            if (_timeLeft == 0)
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
            SceneManager.LoadScene(mainMenuScene);
        }

        private void ServerShutdownToMenu()
        {
            Debug.Log("[GameFlowManager] Server stopping and loading menu locally.");
            base.NetworkManager.ServerManager.StopConnection(false);
            base.NetworkManager.ClientManager.StopConnection();
            SceneManager.LoadScene(mainMenuScene);
        }

        public int GetTimeLeft() => _timeLeft;
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
