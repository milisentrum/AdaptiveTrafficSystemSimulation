using UnityDevKit.Events;
using UnityEngine;

namespace AdaptiveTrafficSystem.Tracking
{
    public interface IDetector
    {
        void Detect(GameObject detectedObject);
        
        void Lose(GameObject detectedObject);

        void SubscribeToDetect(EventHolder<GameObject>.EventHandler listener);
        
        void SubscribeToLose(EventHolder<GameObject>.EventHandler listener);
    }
}