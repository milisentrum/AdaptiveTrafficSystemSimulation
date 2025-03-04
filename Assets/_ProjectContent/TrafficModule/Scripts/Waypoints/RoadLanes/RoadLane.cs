using System;
using System.Collections.Generic;
using System.Linq;
using TrafficModule.Connecting;
using UnityEngine;

namespace TrafficModule.Waypoints.RoadLanes
{
    [Serializable]
    public class RoadLane
    {
        public List<Waypoint> waypoints;
        private List<Waypoint> _helpersList = new List<Waypoint>();

        public void ReverseWaypoints()
        {
            waypoints.Reverse();
        }

        private Waypoint AddNewWaypoint(Waypoint parentWaypoint, Vector3 spawnPosition)
        {
            return WaypointCreator.CreateWaypointAfter(
                parentWaypoint.transform.parent.gameObject,
                parentWaypoint,
                spawnPosition,
                Quaternion.identity, Vector3.zero);
        }

        public void TwoPointsSplitter(Waypoint firstWaypoint, Waypoint secondWaypoint, int newCrossDistance)
        {
            while (true)
            {
                if (Vector3.Distance(
                        firstWaypoint.transform.position,
                        secondWaypoint.transform.position)
                    < 2 * newCrossDistance) break;

                var spawnPosition = Vector3.MoveTowards(
                    firstWaypoint.transform.position,
                    secondWaypoint.transform.position,
                    newCrossDistance);
                var createdWaypoint = AddNewWaypoint(firstWaypoint, spawnPosition);
                _helpersList.Add(createdWaypoint);
                firstWaypoint = createdWaypoint;
            }
        }

        public void Split(int newCrossDistance)
        {
            ReverseWaypoints();
            for (var i = 0; i < waypoints.Count - 1; i++)
            {
                _helpersList.Add(waypoints[i]);
                TwoPointsSplitter(waypoints[i], waypoints[i + 1], newCrossDistance);
            }

            _helpersList.Add(waypoints.Last());
            waypoints = _helpersList.ToList();
            _helpersList.Clear();
        }
    }
}