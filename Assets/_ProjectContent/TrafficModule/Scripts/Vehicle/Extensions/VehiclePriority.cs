using MyBox;
using UnityDevKit.Events;
using UnityEngine;

namespace TrafficModule.Vehicle.Extensions
{
    public class VehiclePriority : MonoBehaviour, IVehicleExtension
    {
        private VehicleController _vehicleController;
        private VehicleNavigator.Path _navigatorPath;
        
        [ReadOnly] public PriorityType Priority; // TODO -- private set

        public readonly EventHolder<PriorityType> OnPriorityUpdate = new EventHolder<PriorityType>();
        public readonly EventHolder<PriorityType> OnPriorityChange = new EventHolder<PriorityType>();

        
        public enum PriorityType
        {
            Default = 0,
            StraightCross = -1,
            LeftCross = -3,
            RightCross = -2,
            BackCross = -5
        }

        protected void Awake()
        {
            _vehicleController = GetComponent<VehicleController>();
        }

        public void Init()
        {
            _navigatorPath = _vehicleController.NavigatorPath;
            _vehicleController.Navigator.OnDestinationReached.AddListener(OnDestinationReached);
        }

        private void OnDestinationReached()
        {
            var newPriority = _navigatorPath.HasNoFuturePath
                ? PriorityType.Default
                : _vehicleController.NavigatorPath.CurrentWaypoint.GetWaypointPriorityType(
                    _navigatorPath.PreviousWaypoint,
                    _navigatorPath.FuturePoints[0]);

            if (Priority != newPriority)
            {
               OnPriorityChange.Invoke(newPriority); 
            }

            Priority = newPriority;
            OnPriorityUpdate.Invoke(Priority);
        }

        public bool HasHigherPriorityThen(VehiclePriority vehiclePriority)
            => (int) Priority > (int) vehiclePriority.Priority;
    }
}