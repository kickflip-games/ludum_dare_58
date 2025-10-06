using LudumDare58.Input;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LudumDare58
{
    public class Vehicle : MonoBehaviour
    {
        [Header("Vehicle Core")]
        [SerializeField] VehicleStats stats = new VehicleStats();
        [SerializeField] VehiclePhysics physics = new VehiclePhysics();
        [SerializeField] VehicleGroundDetection groundDetection = new VehicleGroundDetection();

        VehicleInputManager inputManager = new VehicleInputManager();

        public VehicleStats FinalStats => stats;
        public Rigidbody Rigidbody { get; private set; }
        public Vector2 Input => inputManager.Input;
        public bool IsGrounded => groundDetection.IsGrounded;
        public bool CanMove { get; set; } = true;
        public float ForwardSpeed => Vector3.Dot(Rigidbody.linearVelocity, transform.forward);

        public float NormalizedForwardSpeed =>
            (Mathf.Abs(ForwardSpeed) > 0.1f) ? ForwardSpeed / FinalStats.MaxSpeed : 0.0f;

        // ---------------------------
        // Drift / Handbrake settings
        // ---------------------------
        [Header("Drift Dynamics")]
        [SerializeField] float driftMoveSpeed = 50f;
        [SerializeField] float driftMaxSpeed = 15f;
        [Range(0.5f, 1f)][SerializeField] float driftDrag = 0.98f;
        [SerializeField] float driftSteerAngle = 20f;
        [SerializeField] float driftTraction = 1f;

        [Header("Handbrake Input")]
        [SerializeField] string handbrakeActionName = "Handbrake";
        [SerializeField] bool fallbackToSpacebar = true;
        [SerializeField] bool fallbackToReverseInput = true;

        Vector3 driftVelocity;
        bool isDrifting;
        bool wasDrifting;

        // ---------------------------
        // Auto Upright / Unstuck logic
        // ---------------------------
        [Header("Stuck / Flip Auto-Reset")]
        [SerializeField] float stuckVelocityThreshold = 0.5f;   // m/s below which car may be stuck
        [SerializeField] float stuckTimeThreshold = 3f;         // seconds before we auto-reset
        [SerializeField] float flipDotThreshold = 0.3f;         // how "upside down" we allow
        [SerializeField] float resetCooldown = 5f;              // minimum seconds between resets

        float stuckTimer = 0f;
        float lastResetTime = -10f;



#if ENABLE_INPUT_SYSTEM
        PlayerInput playerInput;
        InputAction handbrakeAction;
#endif

        void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            inputManager.Initialize(this);
            groundDetection.Initialize(this);
        }

        protected virtual void FixedUpdate()
        {
            CollectInput();
            PerformGroundCheck();
            ApplyGravity();
            CheckFlipAndStuck();

            // Handle drift input state
            isDrifting = EvaluateDriftState(Input);

            if (!isDrifting)
            {
                // Normal vehicle physics
                SyncDriftVelocityWithRigidbody();
                wasDrifting = false;

                ApplyLateralFriction();
                ApplySteering();
                ApplyAcceleration();
                return;
            }

            // Begin drift mode if just entered
            if (!wasDrifting)
            {
                driftVelocity = GetPlanarVelocity(Rigidbody.linearVelocity);
                wasDrifting = true;
            }

            ApplyDriftPhysics(Input);



        }

        // ---------------------------
        // Core Vehicle Behavior
        // ---------------------------
        protected virtual void CollectInput() => inputManager.CollectInput();
        protected virtual void PerformGroundCheck() => groundDetection.CheckGround();

        protected virtual void ApplyGravity()
        {
            float factor = groundDetection.IsGrounded ? physics.Gravity : physics.FallGravity;
            Rigidbody.AddForce(-factor * Vector3.up, ForceMode.Acceleration);
        }

        protected virtual void ApplyLateralFriction()
        {
            if (IsGrounded)
            {
                float lateralSpeed = Vector3.Dot(Rigidbody.linearVelocity, transform.right);
                Vector3 lateralFriction = -transform.right * (lateralSpeed / Time.fixedDeltaTime) * FinalStats.Grip;
                Rigidbody.AddForce(lateralFriction, ForceMode.Acceleration);
            }
        }

        protected virtual void ApplySteering()
        {
            if (IsGrounded && CanMove)
            {
                float steeringPower = Input.x * FinalStats.SteeringPower;
                float speedFactor = ForwardSpeed * 0.075f;
                steeringPower = Mathf.Clamp(steeringPower * speedFactor, -FinalStats.SteeringPower, FinalStats.SteeringPower);
                float rotationTorque = steeringPower - Rigidbody.angularVelocity.y;
                Rigidbody.AddRelativeTorque(0f, rotationTorque, 0f, ForceMode.VelocityChange);
            }
        }

        protected virtual void ApplyAcceleration()
        {
            if (IsGrounded && CanMove)
            {
                var force = FinalStats.Acceleration * Input.y;
                Rigidbody.AddForce(transform.forward * force, ForceMode.Acceleration);
            }
        }

        // ---------------------------
        // Drift Physics
        // ---------------------------
        void ApplyDriftPhysics(Vector2 input)
        {
            Vector3 planarForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            float throttle = Mathf.Clamp(input.y, -1f, 1f);

            driftVelocity += planarForward * driftMoveSpeed * throttle * Time.fixedDeltaTime;
            driftVelocity *= driftDrag;
            driftVelocity = Vector3.ClampMagnitude(driftVelocity, driftMaxSpeed);

            float magnitude = driftVelocity.magnitude;
            if (magnitude > 0.0001f)
            {
                Vector3 desiredDir = Vector3.Lerp(driftVelocity / magnitude, planarForward, driftTraction * Time.fixedDeltaTime);
                if (desiredDir.sqrMagnitude > 0.0001f)
                {
                    driftVelocity = desiredDir.normalized * magnitude;
                }
            }

            float steerInput = Mathf.Clamp(input.x, -1f, 1f);
            if (Mathf.Abs(steerInput) > 0.01f && driftVelocity.sqrMagnitude > 0.0001f)
            {
                float yaw = steerInput * driftVelocity.magnitude * driftSteerAngle * Time.fixedDeltaTime;
                Rigidbody.MoveRotation(Rigidbody.rotation * Quaternion.Euler(0f, yaw, 0f));
            }

            float verticalVelocity = Rigidbody.linearVelocity.y;
            Vector3 planar = driftVelocity;
            Rigidbody.linearVelocity = new Vector3(planar.x, verticalVelocity, planar.z);
        }

        bool EvaluateDriftState(Vector2 input)
        {
            if (!IsGrounded || !CanMove)
                return false;

            bool driftPressed = false;
#if ENABLE_INPUT_SYSTEM
            CacheHandbrakeAction();
            if (handbrakeAction != null)
                driftPressed = handbrakeAction.IsPressed();
            else if (fallbackToSpacebar && Keyboard.current != null)
                driftPressed = Keyboard.current.spaceKey.isPressed;
#else
            if (fallbackToSpacebar)
                driftPressed = UnityEngine.Input.GetKey(KeyCode.Space);
#endif

            if (!driftPressed && fallbackToReverseInput)
                driftPressed = input.y < -0.25f;

            return driftPressed;
        }

        void CacheHandbrakeAction()
        {
#if ENABLE_INPUT_SYSTEM
            if (handbrakeAction != null) return;
            playerInput ??= GetComponent<PlayerInput>();
            if (playerInput != null && !string.IsNullOrEmpty(handbrakeActionName))
                handbrakeAction = playerInput.actions?.FindAction(handbrakeActionName, throwIfNotFound: false);
#endif
        }

        void SyncDriftVelocityWithRigidbody() =>
            driftVelocity = GetPlanarVelocity(Rigidbody.linearVelocity);

        static Vector3 GetPlanarVelocity(Vector3 v) =>
            new Vector3(v.x, 0f, v.z);

        // ---------------------------
        // Auto setup & Gizmos
        // ---------------------------
        public void Reset()
        {
            if (GetComponent<Rigidbody>() == null)
            {
                var rb = gameObject.AddComponent<Rigidbody>();
                rb.interpolation = RigidbodyInterpolation.None;
                rb.useGravity = false;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.mass = 2500;
                rb.angularDamping = 3;
                rb.linearDamping = 0.05f;
            }

            if (GetComponent<VehicleInputSource>() == null)
            {
                gameObject.AddComponent<VehiclePlayerInput>();
            }
        }

        // private void OnDrawGizmosSelected() =>
        //     groundDetection.OnDrawGizmosSelected(this);


        public void ResetUpright()
        {
            // Zero out velocity and angular velocity
            Rigidbody.linearVelocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;

            // Smoothly rotate upright while keeping forward direction
            Quaternion uprightRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
            Rigidbody.MoveRotation(uprightRotation);

            // move slightly backwards horizontally, and slightly upwards vertically
            transform.position -= transform.forward * 2f;
            transform.position += Vector3.up * 1f;
        }


        void CheckFlipAndStuck()
        {

            // Skip if recently reset
            if (Time.time - lastResetTime < resetCooldown)
                return;

            // 1. Check if flipped (dot < threshold)
            float uprightDot = Vector3.Dot(transform.up, Vector3.up);
            bool isFlipped = uprightDot < flipDotThreshold;

            // 2. Check if stuck (low speed but grounded with input)
            bool isSlow = Rigidbody.linearVelocity.magnitude < stuckVelocityThreshold;
            bool tryingToMove = Input.magnitude > 0.1f;
            if (IsGrounded && isSlow && tryingToMove)
                stuckTimer += Time.fixedDeltaTime;
            else
                stuckTimer = 0f;

            bool isStuck = stuckTimer > stuckTimeThreshold;


            // 3. Auto reset if flipped or stuck
            if (isFlipped || isStuck)
            {

                Debug.Log($"[Vehicle] Auto reset ({(isFlipped ? "flipped" : "stuck")}) at time {Time.time:F1}");
                Debug.Log($"[Vehicle] Not BEING RESET: flipped={isFlipped} (dot={uprightDot:F2}), stuck={isStuck} (vel={Rigidbody.linearVelocity.magnitude:F2}, tryingToMove={tryingToMove}, isGrounded={IsGrounded}, isSlow={isSlow}, inputMag={Input.magnitude:F2}, stuckTimer={stuckTimer:F1})");

                ResetUpright();
                lastResetTime = Time.time;
                stuckTimer = 0f;
            }

            // every few frames provide some reasoning why not resetting
            if (Time.frameCount % 30 == 0)
                Debug.Log($"[Vehicle] Not resetting: flipped={isFlipped} (dot={uprightDot:F2}), stuck={isStuck} (vel={Rigidbody.linearVelocity.magnitude:F2}, tryingToMove={tryingToMove}, isGrounded={IsGrounded}, isSlow={isSlow}, inputMag={Input.magnitude:F2}, stuckTimer={stuckTimer:F1})");




        }


        private void OnDrawGizmosSelected()
        {
            groundDetection.OnDrawGizmosSelected(this);
        }




    }
    



}
