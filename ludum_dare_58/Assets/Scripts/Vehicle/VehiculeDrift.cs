using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LudumDare58
{
    /// <summary>
    /// Vehicle variant that enables an arcade-style drift while the handbrake is engaged.
    /// Falls back to the base Vehicle handling when drifting is not active.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class VehiculeDrift : Vehicle
    {
        [Header("Drift Dynamics")]
        [Tooltip("Acceleration strength applied while drifting. Mirrors CarController.MoveSpeed.")]
        [SerializeField]
        float moveSpeed = 50f;

        [Tooltip("Maximum planar speed during a drift. Mirrors CarController.MaxSpeed.")]
        [SerializeField]
        float maxSpeed = 15f;

        [Tooltip("Velocity damping applied each frame while drifting. Mirrors CarController.Drag.")]
        [Range(0.5f, 1f)]
        [SerializeField]
        float drag = 0.98f;

        [Tooltip("Steer sharpness while drifting. Mirrors CarController.SteerAngle.")]
        [SerializeField]
        float steerAngle = 20f;

        [Tooltip("How strongly the velocity realigns to the forward direction while drifting. Mirrors CarController.Traction.")]
        [SerializeField]
        float traction = 1f;

        [Header("Handbrake Input")]
        [SerializeField]
        string handbrakeActionName = "Handbrake";

        [Tooltip("Allow the space bar to trigger the drift when no input action is present.")]
        [SerializeField]
        bool fallbackToSpacebar = true;

        [Tooltip("If enabled, holding reverse input will also trigger the drift.")]
        [SerializeField]
        bool fallbackToReverseInput = true;

        Vector3 driftVelocity;
        bool isDrifting;
        bool wasDrifting;

#if ENABLE_INPUT_SYSTEM
        PlayerInput playerInput;
        InputAction handbrakeAction;
#endif

        protected override void FixedUpdate()
        {
            CollectInput();
            PerformGroundCheck();
            ApplyGravity();

            isDrifting = EvaluateDriftState(Input);

            if (!isDrifting)
            {
                SyncDriftVelocityWithRigidbody();
                wasDrifting = false;

                base.ApplyLateralFriction();
                base.ApplySteering();
                base.ApplyAcceleration();
                return;
            }

            if (!wasDrifting)
            {
                driftVelocity = GetPlanarVelocity(Rigidbody.linearVelocity);
                wasDrifting = true;
            }

            ApplyDriftPhysics(Input);
        }

        void ApplyDriftPhysics(Vector2 input)
        {
            Vector3 planarForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            float throttle = Mathf.Clamp(input.y, -1f, 1f);

            driftVelocity += planarForward * moveSpeed * throttle * Time.fixedDeltaTime;

            driftVelocity *= drag;
            driftVelocity = Vector3.ClampMagnitude(driftVelocity, maxSpeed);

            float magnitude = driftVelocity.magnitude;
            if (magnitude > 0.0001f)
            {
                Vector3 desiredDir = Vector3.Lerp(driftVelocity / magnitude, planarForward, traction * Time.fixedDeltaTime);
                if (desiredDir.sqrMagnitude > 0.0001f)
                {
                    driftVelocity = desiredDir.normalized * magnitude;
                }
            }

            float steerInput = Mathf.Clamp(input.x, -1f, 1f);
            if (Mathf.Abs(steerInput) > 0.01f && driftVelocity.sqrMagnitude > 0.0001f)
            {
                float yaw = steerInput * driftVelocity.magnitude * steerAngle * Time.fixedDeltaTime;
                Rigidbody.MoveRotation(Rigidbody.rotation * Quaternion.Euler(0f, yaw, 0f));
            }

            float verticalVelocity = Rigidbody.linearVelocity.y;
            Vector3 planar = driftVelocity;
            Rigidbody.linearVelocity = new Vector3(planar.x, verticalVelocity, planar.z);
        }

        void SyncDriftVelocityWithRigidbody()
        {
            driftVelocity = GetPlanarVelocity(Rigidbody.linearVelocity);
        }

        static Vector3 GetPlanarVelocity(Vector3 velocity)
        {
            return new Vector3(velocity.x, 0f, velocity.z);
        }

        bool EvaluateDriftState(Vector2 input)
        {
            if (!IsGrounded || !CanMove)
            {
                return false;
            }

            bool driftPressed = false;
#if ENABLE_INPUT_SYSTEM
            CacheHandbrakeAction();
            if (handbrakeAction != null)
            {
                driftPressed = handbrakeAction.IsPressed();
            }
            else if (fallbackToSpacebar && Keyboard.current != null)
            {
                driftPressed = Keyboard.current.spaceKey.isPressed;
            }
#else
            if (fallbackToSpacebar)
            {
                driftPressed = UnityEngine.Input.GetKey(KeyCode.Space);
            }
#endif

            if (!driftPressed && fallbackToReverseInput)
            {
                driftPressed = input.y < -0.25f;
            }

            return driftPressed;
        }

        void CacheHandbrakeAction()
        {
#if ENABLE_INPUT_SYSTEM
            if (handbrakeAction != null)
            {
                return;
            }

            playerInput ??= GetComponent<PlayerInput>();
            if (playerInput != null && !string.IsNullOrEmpty(handbrakeActionName))
            {
                handbrakeAction = playerInput.actions?.FindAction(handbrakeActionName, throwIfNotFound: false);
            }
#endif
        }
    }
}
