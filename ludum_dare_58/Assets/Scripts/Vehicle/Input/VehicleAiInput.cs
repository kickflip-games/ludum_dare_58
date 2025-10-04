using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace LudumDare58.Input
{
    [RequireComponent(typeof(Vehicle))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class VehicleAiInput : VehicleInputSource
    {
        [Header("Target Settings")] public Transform target;

        [Header("Navigation Settings")] public float updateInterval = 0.1f;
        public float lookAheadDistance = 10f;

        [Header("Driving Behavior")] public float stoppingDistance = 5f;
        public float reverseDistance = 10f;
        public float maxSteerAngle = 45f;
        public float reverseAngleThreshold = 120f;
        public float reversePower = 1.5f;
        public float brakingSpeed = 5f;

        [Header("Obstacle Avoidance")] public float obstacleDetectionDistance = 5f;
        public float obstacleAvoidanceStrength = 1f;
        public LayerMask obstacleLayer;

        [Header("Stuck Detection")] public float stuckCheckInterval = 1f;
        public float stuckSpeedThreshold = 0.5f;
        public float stuckDuration = 3f;
        public float unstuckReverseDuration = 1.5f;

        private Vehicle vehicle;
        private Vector2 movementInput = Vector2.zero;
        private NavMeshAgent navMeshAgent;
        private float lastUpdateTime;
        private bool isStuck = false;
        private float stuckTime = 0f;
        private Vector3 lastPosition;

        private void Start()
        {
            vehicle = GetComponent<Vehicle>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.updateRotation = false;
            navMeshAgent.updatePosition = false;
            lastPosition = transform.position;
            // StartCoroutine(RepeatedlyCheckStuckStatus());
        }

        public override Vector2 GetInput()
        {
            return movementInput;
        }

        private void Update()
        {
            if (Time.time - lastUpdateTime > updateInterval)
            {
                UpdateMovementInput();
                lastUpdateTime = Time.time;
            }
        }


        // OnCollision RepeatedlyCheckStuckStatus

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log("Collision detected with " + collision.gameObject.name);
            if (collision.gameObject.CompareTag("Obstacle"))
            {
                StartCoroutine(UnstuckRoutine());
            }
        }

        private void UpdateMovementInput()
        {
            if (target == null)
                return;

            navMeshAgent.nextPosition = transform.position;
            navMeshAgent.SetDestination(target.position);

            Vector3 steeringTarget = GetSteeringTarget();
            Vector3 localTarget = transform.InverseTransformPoint(steeringTarget);
            float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

            float steerAmount = Mathf.Clamp(targetAngle / maxSteerAngle, -1f, 1f);
            float forwardAmount = 0f;

            float distanceToTarget =
                Vector3.Distance(transform.position, target.position);

            bool shouldReverse = ShouldReverse(targetAngle, distanceToTarget);

            if (isStuck)
            {
                forwardAmount = -1f * reversePower;
                // steerAmount = Random.Range(-1f, 1f); // Random steering to try to get unstuck
                steerAmount = 0f; // No steering when stuck
            }
            else if (shouldReverse)
            {
                forwardAmount = -1f * reversePower;
                steerAmount = -steerAmount; // Reverse steering when going backwards
            }
            else if (distanceToTarget > stoppingDistance)
            {
                forwardAmount = 1f;
            }
            else
            {
                // Near the target, slow down
                forwardAmount = Mathf.Clamp01(distanceToTarget / stoppingDistance);
            }

            // Apply brakes if we're about to overshoot the target
            if (distanceToTarget < stoppingDistance &&
                vehicle.ForwardSpeed > brakingSpeed && !shouldReverse && !isStuck)
            {
                forwardAmount = -1f;
            }

            // Apply obstacle avoidance
            Vector3 avoidanceVector = GetObstacleAvoidanceVector();
            steerAmount += avoidanceVector.x * obstacleAvoidanceStrength;
            steerAmount = Mathf.Clamp(steerAmount, -1f, 1f);

            movementInput = new Vector2(steerAmount, forwardAmount);
        }

        private bool ShouldReverse(float targetAngle, float distanceToTarget)
        {
            return Mathf.Abs(targetAngle) > reverseAngleThreshold &&
                   distanceToTarget < reverseDistance;
        }

        private Vector3 GetSteeringTarget()
        {
            if (navMeshAgent.path.corners.Length < 2)
                return target.position;

            for (int i = 1; i < navMeshAgent.path.corners.Length; i++)
            {
                if (Vector3.Distance(transform.position, navMeshAgent.path.corners[i]) >
                    lookAheadDistance)
                {
                    return navMeshAgent.path.corners[i];
                }
            }

            return navMeshAgent.path.corners[navMeshAgent.path.corners.Length - 1];
        }

        private Vector3 GetObstacleAvoidanceVector()
        {
            Vector3 avoidanceVector = Vector3.zero;
            RaycastHit hit;

            // Cast rays in a fan shape in front of the vehicle
            for (int i = -2; i <= 2; i++)
            {
                Vector3 rayDirection = Quaternion.Euler(0, i * 30, 0) * transform.forward;
                if (Physics.Raycast(transform.position, rayDirection, out hit,
                        obstacleDetectionDistance, obstacleLayer))
                {
                    avoidanceVector -= hit.normal *
                                       (1 - hit.distance / obstacleDetectionDistance);
                }
            }

            return avoidanceVector.normalized;
        }

        private IEnumerator RepeatedlyCheckStuckStatus()
        {
            while (true)
            {
                yield return new WaitForSeconds(stuckCheckInterval);
                CheckIfStuck();
            }
        }

        void CheckIfStuck()
        {
            Debug.Log("Checking if vehicle is stuck");
            float speed = Vector3.Distance(transform.position, lastPosition) /
                          stuckCheckInterval;
            lastPosition = transform.position;
            if (speed < stuckSpeedThreshold)
            {
                stuckTime += stuckCheckInterval;
                if (stuckTime >= stuckDuration && !isStuck)
                {
                    StartCoroutine(UnstuckRoutine());
                }
            }
            else
            {
                stuckTime = 0f;
            }
        }


        private IEnumerator UnstuckRoutine()
        {
            isStuck = true;
            Debug.Log("Vehicle is stuck, trying to reverse");
            yield return new WaitForSeconds(unstuckReverseDuration);
            isStuck = false;
            Debug.Log("Vehicle unstuck!");
            stuckTime = 0f;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || navMeshAgent == null || !navMeshAgent.hasPath)
                return;

            Gizmos.color = Color.yellow;
            Vector3 previousCorner = transform.position;
            foreach (Vector3 corner in navMeshAgent.path.corners)
            {
                Gizmos.DrawLine(previousCorner, corner);
                previousCorner = corner;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(GetSteeringTarget(), 0.5f);

            // Draw obstacle detection rays
            Gizmos.color = Color.blue;
            for (int i = -2; i <= 2; i++)
            {
                Vector3 rayDirection = Quaternion.Euler(0, i * 30, 0) * transform.forward;
                // if Hit -- Draw a red line
                RaycastHit hit;
                if (Physics.Raycast(transform.position, rayDirection, out hit,
                    obstacleDetectionDistance, obstacleLayer))
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, hit.point);
                }
                else
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawRay(transform.position,
                        rayDirection * obstacleDetectionDistance);
                }
            }

            // Draw if the vehicle is stuck
            if (isStuck)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, 1f);
            }
        }
    }
}