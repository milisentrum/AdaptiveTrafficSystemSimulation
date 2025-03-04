using System.Collections.Generic;
using TrafficModule.Waypoints;
using UnityEngine;

namespace TrafficModule.Connecting
{
    public class ConnectingWaypoints : MonoBehaviour
    {
        public List<Waypoint> start = new List<Waypoint>();
        public List<Waypoint> end = new List<Waypoint>();
        public List<Waypoint> beforeEnd = new List<Waypoint>();
        public List<Waypoint> others = new List<Waypoint>();
    }
}