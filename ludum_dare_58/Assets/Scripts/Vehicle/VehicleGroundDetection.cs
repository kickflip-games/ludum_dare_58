using UnityEngine;

namespace LudumDare58
{
    [System.Serializable]
    public class VehicleGroundDetection
    {
        [Tooltip("Which layers should be handled as ground")]
        public LayerMask GroundLayers = Physics.DefaultRaycastLayers;

        [Tooltip("How far to raycast when checking for ground")]
        public float RaycastDist = 0.25f;

        [Tooltip("Offset from center to front raycast point")]
        public float FrontOffset = 1.5f;

        [Tooltip("Offset from center to back raycast point")]
        public float BackOffset = 1.5f;

        [Tooltip("Offset from center to side raycast points")]
        public float SideOffset = 0.75f;

        Vehicle vehicle;

        /// <summary>
        /// Gets whether the vehicle has been grounded the last time CheckGround was called.
        /// </summary>
        public bool IsGrounded { get; private set; }

        public void Initialize(Vehicle vehicle)
        {
            this.vehicle = vehicle;
        }

        /// <summary>
        /// Checks whether the vehicle is grounded and store the value in the IsGrounded property
        /// </summary>
        public void CheckGround()
        {
            IsGrounded = false;

            // Define raycast points (front-left, front-right, back-left, back-right, center)
            Vector3[] raycastPoints = new Vector3[]
            {
                vehicle.transform.position + vehicle.transform.forward * FrontOffset + vehicle.transform.right * SideOffset,   // Front-right
                vehicle.transform.position + vehicle.transform.forward * FrontOffset - vehicle.transform.right * SideOffset,   // Front-left
                vehicle.transform.position - vehicle.transform.forward * BackOffset + vehicle.transform.right * SideOffset,    // Back-right
                vehicle.transform.position - vehicle.transform.forward * BackOffset - vehicle.transform.right * SideOffset,    // Back-left
                vehicle.transform.position  // Center
            };

            // Check each raycast point - if ANY hit ground, vehicle is grounded
            foreach (Vector3 point in raycastPoints)
            {
                Ray ray = new Ray(point, -vehicle.transform.up);
                if (Physics.Raycast(ray, RaycastDist, GroundLayers))
                {
                    IsGrounded = true;
                    break; // No need to check remaining points
                }
            }
        }

        /// <summary>
        /// Draws rays for debugging purposes (only in editor)
        /// </summary>
        public void OnDrawGizmosSelected(Vehicle vehicle)
        {
#if UNITY_EDITOR
            var direction = -vehicle.transform.up;
            var length = RaycastDist;

            // Draw all raycast points
            Vector3[] raycastPoints = new Vector3[]
            {
                vehicle.transform.position + vehicle.transform.forward * FrontOffset + vehicle.transform.right * SideOffset,
                vehicle.transform.position + vehicle.transform.forward * FrontOffset - vehicle.transform.right * SideOffset,
                vehicle.transform.position - vehicle.transform.forward * BackOffset + vehicle.transform.right * SideOffset,
                vehicle.transform.position - vehicle.transform.forward * BackOffset - vehicle.transform.right * SideOffset,
                vehicle.transform.position
            };

            foreach (Vector3 point in raycastPoints)
            {
                Debug.DrawRay(point, direction * length, Color.magenta);
            }
#endif
        }
    }
}