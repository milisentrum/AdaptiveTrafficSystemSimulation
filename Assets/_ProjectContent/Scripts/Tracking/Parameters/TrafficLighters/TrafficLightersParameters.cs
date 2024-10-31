using System.Collections.Generic;
using System.Linq;
using AdaptiveTrafficSystem.Paths;
using UnityEngine;

namespace AdaptiveTrafficSystem.Tracking.Parameters
{
    public class TrafficLightersParameters : MonoBehaviour
    {
        [SerializeField] private ControlledPath[] controlledPaths;

        public struct LightersPhaseDurations
        {
            public float GreenPhase;
            public float RedPhase;
            public float GreenToRedYellowPhase;
            public float RedToGreenYellowPhase;
        }

        public float GetGreenPhase(PathDirection direction) =>
            GetPathByDirection(direction).TrafficGroup.OpenPathTime.GetValue();

        public float GetRedPhase(PathDirection direction) =>
            controlledPaths
                .Where(path => !path.PathDirections.Contains(direction))
                .Sum(path => path.TrafficGroup.OpenPathTime.GetValue());

        public float GetGreenToRedYellowPhase(PathDirection direction) =>
            GetPathByDirection(direction).TrafficGroup
                .SyncLighters
                .First().GetSwitchingTimeToRed();

        public float GetRedToGreenYellowPhase(PathDirection direction) =>
            GetPathByDirection(direction).TrafficGroup
                .SyncLighters
                .First().GetSwitchingTimeToGreen();

        public LightersPhaseDurations GetLightersPhaseDurations(PathDirection direction) 
            => new LightersPhaseDurations
        {
            GreenPhase = GetGreenPhase(direction),
            RedPhase = GetRedPhase(direction),
            GreenToRedYellowPhase = GetGreenToRedYellowPhase(direction),
            RedToGreenYellowPhase = GetRedToGreenYellowPhase(direction)
        };

        public IEnumerable<LightersPhaseDurations> GetAllLightersPhaseDirections() => // TODO
            controlledPaths
                .SelectMany(path => path.PathDirections
                    .Select(GetLightersPhaseDurations) );


        private ControlledPath GetPathByDirection(PathDirection direction) =>
            controlledPaths.First(path => path.PathDirections.Contains(direction));

        public ControlledPath[] getPaths() => controlledPaths;
    }
}