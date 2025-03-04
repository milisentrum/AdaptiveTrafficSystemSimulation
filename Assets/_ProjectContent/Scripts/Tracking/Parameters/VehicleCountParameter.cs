using AdaptiveTrafficSystem.Tracking;
using AdaptiveTrafficSystem.Tracking.Parameters;
using UnityEngine;

public class VehicleCountParameter : MonoBehaviour, ITrackingParameter
{
    [SerializeField] private TrackerDataSourceBase detectTracker;
    [SerializeField] private TrackerDataSourceBase loseTracker;
    
    private int _currentValue;
    
    private void Start()
    {
        Track();
    }

    private void Track()
    {
        detectTracker.OnDataTracked.AddListener(HandleDetectEvent);
        loseTracker.OnDataTracked.AddListener(HandleLoseEvent);
    }

    
    private void HandleDetectEvent(GameObject detectedObject)
    {
        _currentValue++;
    }
    
    private void HandleLoseEvent(GameObject lostObject)
    {
        _currentValue--;
    }
    
    
    public float GetValue()
    {
        return _currentValue;
    }

    public string GetName() => "Vehicle Count";
}
