using AdaptiveTrafficSystem.Paths;
using MyBox;
using UnityEngine;

namespace AdaptiveTrafficSystem.Tracking.Parameters
{
    public class ParametersHolder : MonoBehaviour
    {
        [Separator("Path settings")] 
        [SerializeField] private string pathName;
        [SerializeField] private PathDirection pathDirection;
        public PathDirection PathDirection => pathDirection;

        [Separator("Intensity")] 
        [SerializeField] private IntensityParameter intensityParameter;
        public IntensityParameter IntensityParameter => intensityParameter;

        [Separator("Speed")] 
        [SerializeField] private AverageSpeedParameter avgSpeedParameter;
        public AverageSpeedParameter AvgSpeedParameter => avgSpeedParameter;

        [SerializeField] private MinSpeedParameter minSpeedParameter;
        public MinSpeedParameter MinSpeedParameter => minSpeedParameter;

        [SerializeField] private MaxSpeedParameter maxSpeedParameter;
        public MaxSpeedParameter MaxSpeedParameter => maxSpeedParameter;

        [Separator("Waiting time")] 
        [SerializeField] private AverageWaitingTimeParameter avgWaitingTimeParameter;
        public AverageWaitingTimeParameter AvgWaitingTimeParameter => avgWaitingTimeParameter;

        [SerializeField] private MinWaitingTimeParameter minWaitingTimeParameter;
        public MinWaitingTimeParameter MinWaitingTimeParameter => minWaitingTimeParameter;

        [SerializeField] private MaxWaitingTimeParameter maxWaitingTimeParameter;
        public MaxWaitingTimeParameter MaxWaitingTimeParameter => maxWaitingTimeParameter;

        [Separator("Vehicle Count")] 
        [SerializeField] private VehicleCountParameter vehicleCountParameter;
        public VehicleCountParameter VehicleCountParameter => vehicleCountParameter;

        [Separator("Service Intensity")] 
        [SerializeField] private ServiceIntensityParameter serviceIntensityParameter;
        public ServiceIntensityParameter ServiceIntensityParameter => serviceIntensityParameter;
        
        [Separator("Pedestrians Count")] 
        [SerializeField] private WaitingPedestriansCountParameter waitingPedestriansCountParameter;
        public WaitingPedestriansCountParameter WaitingPedestriansCountParameter => waitingPedestriansCountParameter;
        
        [SerializeField] private CrossingPedestriansCountParameter crossingPedestriansCountParameter;
        public CrossingPedestriansCountParameter CrossingPedestriansCountParameter => crossingPedestriansCountParameter;
        
        private void DebugParameters()
        {
            Debug.Log($"Path name: {pathName}\n" +
                      $"{intensityParameter.GetName()}: {intensityParameter.GetValue()}\n" +
                      $"{avgSpeedParameter.GetName()}: {avgSpeedParameter.GetValue()}\n" +
                      $"{minSpeedParameter.GetName()}: {minSpeedParameter.GetValue()}\n" +
                      $"{maxSpeedParameter.GetName()}: {maxSpeedParameter.GetValue()}\n" +
                      $"{avgWaitingTimeParameter.GetName()}: {avgWaitingTimeParameter.GetValue()}\n" +
                      $"{minWaitingTimeParameter.GetName()}: {minWaitingTimeParameter.GetValue()}\n" +
                      $"{maxWaitingTimeParameter.GetName()}: {maxWaitingTimeParameter.GetValue()}\n" +
                      $"{serviceIntensityParameter.GetName()}: {serviceIntensityParameter.GetDirectionValue(pathDirection)}\n" +
                      $"{vehicleCountParameter.GetName()}: {vehicleCountParameter.GetValue()}\n" +
                      $"{waitingPedestriansCountParameter.GetName()}: {waitingPedestriansCountParameter.GetValue()}\n" +
                      $"{crossingPedestriansCountParameter.GetName()}: {crossingPedestriansCountParameter.GetValue()}\n");
        }
    }
}