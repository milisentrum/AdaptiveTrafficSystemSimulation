using MyBox;
using UnityEngine;

namespace AdaptiveTrafficSystem.Pedestrians.Data
{
    [CreateAssetMenu(fileName = "MovementData", menuName = "Pedestrians/MovementData", order = 0)]
    public class MovementData : ScriptableObject
    {
        [Separator("Movement parameters")] 
        public float Speed = 3.5f;
        public float AngularSpeed = 120;
        public float Acceleration = 8;
        public float StoppingDistance = 0;
        public bool AutoBraking = true;
        public AnimationCurve SpeedChangeCurve;

        [Separator("Obstacle avoidance")] 
        public float SocialDistance = 0.5f;
        public float Height = 2;
    }
}