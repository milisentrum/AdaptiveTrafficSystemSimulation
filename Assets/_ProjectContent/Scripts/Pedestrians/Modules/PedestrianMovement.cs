using System.Collections;
using AdaptiveTrafficSystem.Pedestrians.Data;
using UnityEngine;
using UnityEngine.AI;
using MyBox;
using UnityDevKit.Optimization;

namespace AdaptiveTrafficSystem.Pedestrians.Modules
{
    [RequireComponent(typeof(Pedestrian))]
    public class PedestrianMovement : CachedMonoBehaviour, IPedestrianModule
    {
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] [DisplayInspector] private MovementData agentMovementData;
        [SerializeField] private bool accelerationAdvancedControl;

        public Vector2 Velocity { get; private set; } = Vector2.zero;
        public bool IsMoving { get; private set; } = false;
        public bool IsOnDestination =>
            navMeshAgent.hasPath &&
            navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + navMeshAgent.radius;
        public bool HasPath => navMeshAgent.hasPath;
        

        private Vector2 _smoothDeltaPosition = Vector2.zero;
        private Coroutine _currentCoroutine;

        // ----- Acceleration -----
        private const float ACCELERATION_MODIFIER = 0.5f;
        private const float MIN_ACCELERATION = 8;
        private const float MAX_ACCELERATION = 55;

        private void Update()
        {
            const float isMovingMinSpeed = 0.5f;
            var worldDeltaPosition = navMeshAgent.nextPosition - CachedTransform.position;

            // Map 'worldDeltaPosition' to local space
            var dx = Vector3.Dot(CachedTransform.right, worldDeltaPosition);
            var dy = Vector3.Dot(CachedTransform.forward, worldDeltaPosition);
            var deltaPosition = new Vector2(dx, dy);

            // Low-pass filter the deltaMove
            var smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
            _smoothDeltaPosition = Vector2.Lerp(_smoothDeltaPosition, deltaPosition, smooth);

            // Update velocity if time advances
            if (Time.deltaTime > 1e-5f)
            {
                Velocity = _smoothDeltaPosition / Time.deltaTime;
            }

            IsMoving = Velocity.magnitude > isMovingMinSpeed; //&& navMeshAgent.remainingDistance > navMeshAgent.radius;

            if (accelerationAdvancedControl && navMeshAgent.hasPath)
            {
                var toTarget = navMeshAgent.steeringTarget - SelfPosition;
                var turnAngle = Vector3.Angle(CachedTransform.forward, toTarget);
                navMeshAgent.acceleration = Mathf.Clamp(
                    turnAngle * navMeshAgent.speed * ACCELERATION_MODIFIER,
                    MIN_ACCELERATION, MAX_ACCELERATION);
            }
           
            CachedTransform.position = navMeshAgent.nextPosition;
        }

        public void Init()
        {
            ApplyAgentMovementData(agentMovementData);
            navMeshAgent.updatePosition = false; // Don’t update position automatically
        }
        
        public void ApplyAgentMovementData(MovementData data)
        {
            navMeshAgent.speed = data.Speed;
            navMeshAgent.angularSpeed = data.AngularSpeed;
            navMeshAgent.acceleration = data.Acceleration;
            navMeshAgent.stoppingDistance = data.StoppingDistance;
            navMeshAgent.autoBraking = data.AutoBraking;
            navMeshAgent.radius = data.SocialDistance;
            navMeshAgent.height = data.Height;
        }

        public void GoTo(Vector3 point)
        {
            navMeshAgent.destination = point;
        }

        public void SlowDown()
        {
            if (_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
            }
            _currentCoroutine = StartCoroutine(SlowingProcess());
        }

        public void Accelerate()
        {
            if (_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
            }
            _currentCoroutine = StartCoroutine(AcceleratingProcess());
        }

        public void HurryUp()
        {
            const float hurryUpModifier = 1.75f;
            navMeshAgent.speed = agentMovementData.Speed * hurryUpModifier;
        }

        public void SetNormalSpeed()
        {
            navMeshAgent.speed = agentMovementData.Speed;
        }

        private IEnumerator SlowingProcess()
        {
            yield return SpeedChangeProcess(0);
        }

        private IEnumerator AcceleratingProcess()
        {
            yield return SpeedChangeProcess(agentMovementData.Speed);
        }

        private IEnumerator SpeedChangeProcess(float targetSpeed)
        {
            const float duration = 0.5f;
            const float period = 0.05f;
            const float iterations = duration / period;
            const float iterationsMaxIndex = iterations - 1;
            
            var speedDelta = (targetSpeed - navMeshAgent.speed) / iterations;
            for (var i = 0; i < iterations; i++)
            {
                navMeshAgent.speed += speedDelta * agentMovementData.SpeedChangeCurve.Evaluate(i / iterationsMaxIndex) * 2;  
                yield return new WaitForSeconds(period);
            }

            navMeshAgent.speed = targetSpeed;
        }
    }
}