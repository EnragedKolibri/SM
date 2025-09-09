using UnityEngine;
using FishNet.Object;

namespace SM.Player
{
    /// <summary>
    /// Source-like controller: auto-bhop, air accel, surf layer + slope>=35 considered surf.
    /// Client-authoritative; server can teleport.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class SurfBhopMotor : NetworkBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float maxGroundSpeed = 7.0f;
        [SerializeField] private float maxAirSpeed = 10.0f;
        [SerializeField] private float groundAccel = 14.0f;
        [SerializeField] private float airAccel = 80.0f;
        [SerializeField] private float friction = 6.0f;
        [SerializeField] private float surfFriction = 0.5f;
        [SerializeField] private float gravity = 30.0f;
        [SerializeField] private float jumpSpeed = 8.0f;
        [SerializeField] private LayerMask surfLayer;
        [SerializeField] private float surfSlopeMin = 35f;

        [Header("Look")]
        [SerializeField] private Transform viewRoot;
        [SerializeField] private float mouseSensitivity = 1.5f;
        [SerializeField] private float fov = 100f;

        [Header("Toggles/Caps")]
        [SerializeField] private bool autoBhop = true;  // sv_autobhop
        [SerializeField] private float airCap = 0f;     // sv_aircap (0 = uncapped)

        private CharacterController _cc;
        private Vector3 _vel;
        private bool _acceptInput;
        private float _yaw, _pitch;
        private Camera _cam;

        public float LateralSpeed => new Vector3(_vel.x, 0f, _vel.z).magnitude;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _cam = GetComponentInChildren<Camera>(true);
            if (_cam != null) _cam.fieldOfView = fov;
            Debug.Log("[SurfBhopMotor] Awake.");
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            _acceptInput = IsOwner;
            Debug.Log($"[SurfBhopMotor] StartClient input={_acceptInput}");
            if (IsOwner)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void Update()
        {
            if (IsOwner)
                ReadLook();

            TickMove(Time.deltaTime);
        }

        private void ReadLook()
        {
            float mx = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float my = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
            _yaw += mx; _pitch -= my;
            _pitch = Mathf.Clamp(_pitch, -89f, 89f);
            if (viewRoot) viewRoot.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        private void TickMove(float dt)
        {
            Vector2 input = Vector2.zero;
            bool jumpPressed = false;
            bool jumpHeld = false;

            if (_acceptInput)
            {
                input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                input = Vector2.ClampMagnitude(input, 1f);
                jumpHeld = Input.GetKey(KeyCode.Space);
                jumpPressed = Input.GetKeyDown(KeyCode.Space);
            }

            Vector3 fwd = viewRoot ? Vector3.ProjectOnPlane(viewRoot.forward, Vector3.up).normalized : Vector3.forward;
            Vector3 right = viewRoot ? viewRoot.right : Vector3.right;
            Vector3 wishDir = (fwd * input.y + right * input.x).normalized;

            bool grounded = _cc.isGrounded;
            bool onSurf = OnSurf();

            if (grounded && !onSurf)
            {
                GroundMove(wishDir, dt);
                if ((autoBhop && jumpHeld) || (!autoBhop && jumpPressed))
                {
                    _vel.y = jumpSpeed;
                    grounded = false;
                }
            }
            else
            {
                AirMove(wishDir, dt, onSurf);
            }

            _vel.y -= gravity * dt;

            if (airCap > 0f && !grounded)
            {
                Vector3 lat = new Vector3(_vel.x, 0, _vel.z);
                float sp = lat.magnitude;
                if (sp > airCap)
                {
                    lat = lat.normalized * airCap;
                    _vel.x = lat.x; _vel.z = lat.z;
                }
            }

            _cc.Move(_vel * dt);

            if ((_cc.collisionFlags & CollisionFlags.Above) != 0 && _vel.y > 0f)
                _vel.y = 0f;
        }

        private void GroundMove(Vector3 wishDir, float dt)
        {
            ApplyFriction(friction, dt);
            Accelerate(wishDir, maxGroundSpeed, groundAccel, dt);
            _vel.y = -2f;
        }

        private void AirMove(Vector3 wishDir, float dt, bool onSurf)
        {
            if (onSurf) ApplyFriction(surfFriction, dt);
            Accelerate(wishDir, maxAirSpeed, airAccel, dt);
        }

        private void ApplyFriction(float coeff, float dt)
        {
            Vector3 lat = new Vector3(_vel.x, 0, _vel.z);
            float sp = lat.magnitude; if (sp < 0.001f) return;
            float drop = sp * coeff * dt;
            float ns = Mathf.Max(sp - drop, 0f);
            if (ns != sp)
            {
                Vector3 dir = lat / sp;
                lat = dir * ns;
                _vel.x = lat.x; _vel.z = lat.z;
            }
        }

        private void Accelerate(Vector3 wishDir, float wishSpeed, float accel, float dt)
        {
            if (wishDir.sqrMagnitude < 0.0001f) return;
            Vector3 lat = new Vector3(_vel.x, 0, _vel.z);
            float current = Vector3.Dot(lat, wishDir);
            float add = wishSpeed - current;
            if (add <= 0f) return;

            float accelSpeed = accel * dt * wishSpeed;
            if (accelSpeed > add) accelSpeed = add;

            _vel.x += wishDir.x * accelSpeed;
            _vel.z += wishDir.z * accelSpeed;
        }

        private bool OnSurf()
        {
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            if (Physics.Raycast(origin, Vector3.down, out var hit, 0.4f, surfLayer, QueryTriggerInteraction.Ignore))
            {
                float slope = Vector3.Angle(hit.normal, Vector3.up);
                return slope >= surfSlopeMin;
            }
            return false;
        }

        // ---- Console setters (bound by MotorConvarBinder) ----
        public void set_maxGroundSpeed(float v){ maxGroundSpeed = v; }
        public void set_airAccel(float v){ airAccel = v; }
        public void set_groundAccel(float v){ groundAccel = v; }
        public void set_friction(float v){ friction = v; }
        public void set_surfFriction(float v){ surfFriction = v; }
        public void set_gravity(float v){ gravity = v; }
        public void set_airCap(float v){ airCap = v; }
        public void set_autoBhop(int i){ autoBhop = i != 0; }

        // ---- Teleport (server) ----
        [ServerRpc(RequireOwnership = false)]
        public void ServerTeleportRpc(Vector3 pos, Vector3 forward)
        {
            ServerTeleport(pos, forward);
            ObserversTeleport(pos, forward);
        }

        public void ServerTeleport(Vector3 pos, Vector3 forward)
        {
            transform.position = pos;
            _vel = Vector3.zero;
            if (viewRoot != null)
            {
                _yaw = Quaternion.LookRotation(forward, Vector3.up).eulerAngles.y;
                viewRoot.rotation = Quaternion.Euler(0f, _yaw, 0f);
            }
            Debug.Log($"[SurfBhopMotor] ServerTeleport {pos}");
        }

        [ObserversRpc]
        private void ObserversTeleport(Vector3 pos, Vector3 forward)
        {
            if (IsOwner) return;
            transform.position = pos;
            _vel = Vector3.zero;
        }
    }
}
