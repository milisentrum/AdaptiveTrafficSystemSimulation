using System.Collections.Generic;
using AdaptiveTrafficSystem.Paths;
using AdaptiveTrafficSystem.Pedestrians.Modules;
using MyBox;
using UnityEngine;

namespace AdaptiveTrafficSystem.Crossing
{
    [RequireComponent(typeof(CrossingNavMesh))]
    [RequireComponent(typeof(CrossingBuilder))]
    public class Crossing : MonoBehaviour
    {
        [SerializeField] [ReadOnly] private bool hasPedestrians;
        
        private CrossingNavMesh _crossingNavMesh;
        private CrossingBuilder _crossingBuilder;
        private readonly List<PedestrianStates> _crossingPedestrians = new List<PedestrianStates>();
        private readonly List<PedestrianStates> _waitingPedestrians = new List<PedestrianStates>();
        
        private CrossingLinesParameters Parameters => _crossingBuilder.Parameters;
        public bool HasManyPedestrians => hasPedestrians && _crossingPedestrians.Count > MANY_PEDESTRIANS_COUNT;
        public int CrossingPedestriansCount => _crossingPedestrians.Count;
        public int WaitingPedestriansCount => _waitingPedestrians.Count;
        
        public bool IsClosed { get; private set; }

        private const int MANY_PEDESTRIANS_COUNT = 3;
        
        private void Awake()
        {
            _crossingNavMesh = GetComponent<CrossingNavMesh>();
            _crossingBuilder = GetComponent<CrossingBuilder>();
        }

        public void Setup(RoadData data)
        {
            _crossingBuilder.Build(data);
            _crossingNavMesh.Setup(data, Parameters);
        }

        protected void Open()
        {
            IsClosed = false;
        }

        protected void Close()
        {
            IsClosed = true;
        }

        public void AddCrossingPedestrian(PedestrianStates pedestrianStates)
        {
            _crossingPedestrians.Add(pedestrianStates);
            hasPedestrians = true;
        }
        
        public void RemoveCrossingPedestrian(PedestrianStates pedestrianStates)
        {
            _crossingPedestrians.Remove(pedestrianStates);
            hasPedestrians = _crossingPedestrians.Count > 0;
        }
        
        public void AddWaitingPedestrian(PedestrianStates pedestrianStates)
        {
            _waitingPedestrians.Add(pedestrianStates);
        }
        
        public void RemoveWaitingPedestrian(PedestrianStates pedestrianStates)
        {
            _waitingPedestrians.Remove(pedestrianStates);
        }
    }
}