using UnityEngine;

namespace SM.Player
{
    /// <summary>
    /// Polls ConsoleVarManager each frame and applies values to SurfBhopMotor.
    /// </summary>
    public class MotorConvarBinder : MonoBehaviour
    {
        [SerializeField] private SurfBhopMotor motor;
        [SerializeField] private SM.Net.ConsoleVarManager convars;

        private void Update()
        {
            if (motor == null || convars == null) return;
            motor.set_maxGroundSpeed(convars.GetFloat("sv_maxspeed", 7f));
            motor.set_airAccel(convars.GetFloat("sv_airaccelerate", 80f));
            motor.set_groundAccel(convars.GetFloat("sv_accelerate", 14f));
            motor.set_friction(convars.GetFloat("sv_friction", 6f));
            motor.set_surfFriction(convars.GetFloat("sv_surf_friction", 0.5f));
            motor.set_gravity(convars.GetFloat("sv_gravity", 30f));
            motor.set_airCap(convars.GetFloat("sv_aircap", 0f));
            motor.set_autoBhop(convars.GetInt("sv_autobhop", 1));
        }
    }
}
