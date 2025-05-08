using System.Collections.Generic;
using UnityEngine;
using AdaptiveTrafficSystem.Pedestrians.Modules;
using MyBox;
using AdaptiveTrafficSystem.TrafficLighters;

namespace AdaptiveTrafficSystem.Pedestrians
{
    public class PedestriansNavigationSystem : MonoBehaviour
    {
        [SerializeField][InitializationField] private List<PedestrianNavigator> controlledPedestrians;
        [SerializeField] private List<Transform> pathPoints;

        private void Start()
        {
            Init();
            InitDestinations();
        }

        public void AddControlledPedestrian(GameObject pedestrian)
        {
            AddControlledPedestrian(pedestrian.GetComponentInChildren<PedestrianNavigator>());
        }

        public void AddControlledPedestrian(PedestrianNavigator pedestrianNavigator)
        {
            if (controlledPedestrians.Contains(pedestrianNavigator)) return;
            controlledPedestrians.Add(pedestrianNavigator);
            pedestrianNavigator.OnDestinationReached.AddListener(GiveNewDestination);
        }

        public void RemoveControlledPedestrian(GameObject pedestrian)
        {
            RemoveControlledPedestrian(pedestrian.GetComponentInChildren<PedestrianNavigator>());
        }

        public void RemoveControlledPedestrian(PedestrianNavigator pedestrianNavigator)
        {
            if (!controlledPedestrians.Contains(pedestrianNavigator)) return;

            controlledPedestrians.Remove(pedestrianNavigator);
            pedestrianNavigator.OnDestinationReached.RemoveListener(GiveNewDestination);
        }

        public void AddControlledPedestrianWithDestination(GameObject pedestrian)
        {
            var pedestrianNavigator = pedestrian.GetComponentInChildren<PedestrianNavigator>();
            AddControlledPedestrian(pedestrianNavigator);
            GiveNewDestination(pedestrianNavigator);
        }

        public void GiveNewDestination(PedestrianNavigator pedestrianNavigator)
        {
            pedestrianNavigator.NavigateToPoint(SelectRandomPoint());
        }

        private void Init()
        {
            foreach (var pedestrianNavigator in controlledPedestrians)
            {
                pedestrianNavigator.OnDestinationReached.AddListener(GiveNewDestination);
            }
        }

        private void InitDestinations()
        {
            foreach (var pedestrianNavigator in controlledPedestrians)
            {
                GiveNewDestination(pedestrianNavigator);
            }
        }

        private Vector3 SelectRandomPoint() => pathPoints.GetRandom().position;
    }
}

