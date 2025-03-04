using System;
using System.Linq;
using AdaptiveTrafficSystem.Paths;
using UnityEngine;

namespace AdaptiveTrafficSystem.Api
{
    public class TrafficLightersPhases
    {
        public ControlledPath[] ControlledPaths { get; }
        
        public TrafficLightersPhases(ControlledPath[] paths)
        {
            ControlledPaths = paths;
        }

        public void SetPhase(string id, float phase)
        {
            var path = ControlledPaths.FirstOrDefault(path => path.Id == id);

            if (path == null)
            {
                Debug.LogError($"Unknown path with id {id}");
            }
            else
            {
                path.TrafficGroup.OpenPathTime.SetValue(phase);
            }
        }

        public float GetPhase(string id)
        {
            var path = ControlledPaths.FirstOrDefault(path => path.Id == id);
            if (path == null)
            {
                throw new NullReferenceException($"Unknown path with id {id}");
            }

            return path.TrafficGroup.OpenPathTime.GetValue();
        }
    }
}