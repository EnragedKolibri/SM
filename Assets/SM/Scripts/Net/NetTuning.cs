// Assets/SM/Scripts/Net/NetTuning.cs
using UnityEngine;
using FishNet.Managing;

namespace SM.Net
{
    /// <summary>
    /// Sets tick/send rates and interpolation guidance.
    /// </summary>
    public class NetTuning : MonoBehaviour
    {
        [SerializeField] private NetworkManager fishNet;
        [SerializeField] private int tickRate = 60;   // simulation Hz
        [SerializeField] private int sendRate = 60;   // transforms Hz (set on NetworkTransform)
        [SerializeField] private int interpMs = 100;  // client interp buffer

        private void Awake()
        {
            Application.targetFrameRate = 144;
            Time.fixedDeltaTime = 1f / Mathf.Max(1, tickRate);
            Debug.Log($"[NetTuning] tick={tickRate} send={sendRate} interp={interpMs}ms");

            if (fishNet != null)
                fishNet.TimeManager.SetTickRate((ushort)Mathf.Clamp(tickRate, 1, 1024)); // cast to ushort for new API
        }
    }
}