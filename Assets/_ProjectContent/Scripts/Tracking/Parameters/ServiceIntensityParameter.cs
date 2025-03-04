using AdaptiveTrafficSystem.Paths;
using AdaptiveTrafficSystem.Tracking.Parameters;
using UnityEngine;

public class ServiceIntensityParameter : MonoBehaviour
{
    [SerializeField] private AverageWaitingTimeParameter averageWaitingTimeParameter;

    [SerializeField] private TrafficLightersParameters trafficLightersParameters;


    //both parameters should belong to one pathDirection (averageWaitingTime ???)
    public float GetDirectionValue(PathDirection direction) => averageWaitingTimeParameter.GetValue() == 0
        ? 0
        : trafficLightersParameters.GetGreenPhase(direction) /
          ((trafficLightersParameters.GetGreenPhase(direction) +
            trafficLightersParameters.GetRedPhase(direction)) *
           averageWaitingTimeParameter.GetValue()); // TODO create for multiple directions

    public string GetName() => "Service Intensity";
}