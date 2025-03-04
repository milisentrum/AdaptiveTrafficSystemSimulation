using TrafficModule.Waypoints;
using UnityEngine;

namespace TrafficModule.Connecting
{
    public static class WaypointCreator
    {
        private const string WAYPOINT_TAG = "Waypoint";
        
        public static Waypoint CreateWaypoint(
            GameObject parent, 
            Vector3 position, 
            Quaternion rotation,
            Vector3 offset = default)
        {
            var waypointObj = new GameObject("Waypoint " + parent.transform.childCount, typeof(Waypoint))
            {
                tag = WAYPOINT_TAG
            };
            waypointObj.transform.SetParent(parent.transform, false);
            waypointObj.transform.position = position;
            waypointObj.transform.rotation = rotation;
            waypointObj.transform.Translate(offset, Space.Self);
            var newWaypoint = waypointObj.GetComponent<Waypoint>();
            WaypointManager.Instance.allWaypoints.Add(newWaypoint);
            return newWaypoint;
        }

        public static Waypoint CreateBranch(
            GameObject parent, 
            Waypoint waypointFrom, 
            Vector3 position,
            Quaternion rotation,
            Vector3 offset = default)
        {
            var waypointObj = new GameObject("Waypoint " + parent.transform.childCount, typeof(Waypoint))
            {
                tag = WAYPOINT_TAG
            };
            waypointObj.transform.SetParent(parent.transform, false);

            var newWaypoint = waypointObj.GetComponent<Waypoint>();

            waypointFrom.branches.Add(newWaypoint);
            newWaypoint.previousBranches.Add(waypointFrom);
            newWaypoint.transform.position = position;
            newWaypoint.transform.rotation = rotation;
            waypointObj.transform.Translate(offset, Space.Self);
            WaypointManager.Instance.allWaypoints.Add(newWaypoint);
            return newWaypoint;
        }

        public static Waypoint CreateWaypointBefore(
            GameObject parent, 
            Waypoint waypointFrom, 
            Vector3 position,
            Quaternion rotation,
            Vector3 offset = default)
        {
            var waypointObj = new GameObject("Waypoint " + parent.transform.childCount, typeof(Waypoint))
            {
                tag = WAYPOINT_TAG
            };
            waypointObj.transform.SetParent(parent.transform, false);

            var newWaypoint = waypointObj.GetComponent<Waypoint>();

            waypointObj.transform.position = position;
            waypointObj.transform.rotation = rotation;
            waypointObj.transform.Translate(offset, Space.Self);

            if (waypointFrom.previous != null)
            {
                newWaypoint.previous = waypointFrom.previous;
                waypointFrom.previous.next = newWaypoint;
            }

            newWaypoint.next = waypointFrom;
            waypointFrom.previous = newWaypoint;

            newWaypoint.transform.SetSiblingIndex(waypointFrom.transform.GetSiblingIndex());
            WaypointManager.Instance.allWaypoints.Add(newWaypoint);
            return newWaypoint;
        }

        public static Waypoint CreateWaypointAfter(
            GameObject parent, 
            Waypoint waypointFrom, 
            Vector3 position,
            Quaternion rotation,
            Vector3 offset = default)
        {
            var waypointObj = new GameObject("Waypoint " + parent.transform.childCount, typeof(Waypoint))
            {
                tag = WAYPOINT_TAG
            };
            waypointObj.transform.SetParent(parent.transform, false);

            var newWaypoint = waypointObj.GetComponent<Waypoint>();

            waypointObj.transform.position = position;
            waypointObj.transform.rotation = rotation;
            waypointObj.transform.Translate(offset, Space.Self);

            newWaypoint.previous = waypointFrom;

            if (waypointFrom.next != null)
            {
                waypointFrom.next.previous = newWaypoint;
                newWaypoint.next = waypointFrom.next;
            }

            waypointFrom.next = newWaypoint;

            newWaypoint.transform.SetSiblingIndex(waypointFrom.transform.GetSiblingIndex());
            WaypointManager.Instance.allWaypoints.Add(newWaypoint);
            return newWaypoint;
        }

        public static void DeleteWaypoint(Waypoint waypoint)
        {
            if (waypoint.next != null)
            {
                waypoint.next.previous = waypoint.previous;
                waypoint.next.RemoveFromAllPrevious(waypoint);
            }

            if (waypoint.previous != null)
            {
                waypoint.previous.next = waypoint.next;
                waypoint.previous.RemoveFromAllFuture(waypoint);
            }

            if (waypoint.previousBranches.Count != 0)
            {
                foreach (var previous in waypoint.previousBranches)
                {
                    previous.branches.Remove(waypoint);
                    previous.RemoveFromAllFuture(waypoint);
                }
            }

            if (waypoint.branches.Count != 0)
            {
                foreach (var next in waypoint.branches)
                {
                    next.previousBranches.Remove(waypoint);
                    next.RemoveFromAllPrevious(waypoint);
                }
            }

            WaypointManager.Instance.allWaypoints.Remove(waypoint);
            Object.DestroyImmediate(waypoint.gameObject);
        }
    }
}