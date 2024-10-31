using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace TrafficModule.Waypoints.RoadLanes
{
    public class RoadLanesConnector : MonoBehaviour
    {
        [SerializeField] private List<RoadLane> roadLanes;
        [SerializeField] [PositiveValueOnly] private int newCrossDistance = 15;

#if UNITY_EDITOR
        [ButtonMethod]
        public void SplitLanes()
        {
            Debug.Log("Start splitting lanes");
            foreach (var roadLane in roadLanes)
            {
                roadLane.Split(newCrossDistance);
            }
            Debug.Log("Complete splitting lanes");
        }

        [ButtonMethod]
        public void CreateLanesCrossings()
        {
            Debug.Log("Start creating lanes crossings");
            for (var i = 0; i < roadLanes.Count; i++)
            {
                if (i == 0) ConnectLanes(roadLanes[i], roadLanes[i + 1]);
                else if (i == roadLanes.Count - 1) ConnectLanes(roadLanes[i], roadLanes[i - 1]);
                else
                {
                    ConnectLanes(roadLanes[i], roadLanes[i - 1]);
                    ConnectLanes(roadLanes[i], roadLanes[i + 1]);
                }
            }

            Debug.Log("Complete creating lanes crossings");
        }
        
        [ButtonMethod]
        public void RemoveLanesCrossings()
        {
            Debug.Log("Start removing lanes crossings");
            for (var i = 0; i < roadLanes.Count; i++)
            {
                if (i == 0) RemoveLanes(roadLanes[i], roadLanes[i + 1]);
                else if (i == roadLanes.Count - 1) RemoveLanes(roadLanes[i], roadLanes[i - 1]);
                else
                {
                    RemoveLanes(roadLanes[i], roadLanes[i - 1]);
                    RemoveLanes(roadLanes[i], roadLanes[i + 1]);
                }
            }

            Debug.Log("Complete removing lanes crossings");
        }
#endif

        private static void ConnectLanes(RoadLane firstRoadLane, RoadLane secondRoadLane)
        {
            if (firstRoadLane.waypoints.Count != secondRoadLane.waypoints.Count) return;
            for (var i = 0; i < firstRoadLane.waypoints.Count - 1; i++)
            {
                var firstWaypoint = firstRoadLane.waypoints[i];
                var secondWaypoint = secondRoadLane.waypoints[i + 1];

                if (firstWaypoint.intersection || secondWaypoint.intersection) continue;
                CreateBranch(firstWaypoint, secondWaypoint);
            }
        }
        
        private static void CreateBranch(Waypoint firstWaypoint, Waypoint secondWaypoint)
        {
            firstWaypoint.laneChangeBranches.Add(secondWaypoint);
            secondWaypoint.previousBranches.Add(firstWaypoint);
        }
        
        private static void RemoveLanes(RoadLane firstRoadLane, RoadLane secondRoadLane)
        {
            if (firstRoadLane.waypoints.Count != secondRoadLane.waypoints.Count) return;
            for (var i = 0; i < firstRoadLane.waypoints.Count - 1; i++)
            {
                var firstWaypoint = firstRoadLane.waypoints[i];
                var secondWaypoint = secondRoadLane.waypoints[i + 1];

                if (firstWaypoint.intersection || secondWaypoint.intersection) continue;
                RemoveBranch(firstWaypoint, secondWaypoint);
            }
        }

        private static void RemoveBranch(Waypoint firstWaypoint, Waypoint secondWaypoint)
        {
            firstWaypoint.laneChangeBranches.Remove(secondWaypoint);
            secondWaypoint.previousBranches.Remove(firstWaypoint);
        }
    }
}