using UnityEngine;

namespace SM.Net
{
    /// <summary>
    /// Defines a box area for initial spawns and respawns. Forward is this transform's +Z.
    /// </summary>
    public class StartZone : MonoBehaviour
    {
        [SerializeField] private Vector3 size = new Vector3(8, 2, 8);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, size);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero, Vector3.forward * 3);
        }

        public Vector3 GetRandomPoint()
        {
            Vector3 half = size * 0.5f;
            Vector3 local = new Vector3(
                Random.Range(-half.x, half.x),
                0.2f,
                Random.Range(-half.z, half.z));
            return transform.TransformPoint(local);
        }

        public Vector3 Forward() => transform.forward;
    }
}
