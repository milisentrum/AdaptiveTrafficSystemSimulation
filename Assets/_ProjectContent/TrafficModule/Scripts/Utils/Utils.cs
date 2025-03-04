using TrafficModule.Waypoints;
using UnityEngine;

namespace TrafficModule.Utils
{
    public static class Utils
    {
        public static Waypoint GetClosestWaypoint(GameObject gameObject)
        {
            const string waypointTag = "Waypoint";
            var objects = GameObject.FindGameObjectsWithTag(waypointTag);
            GameObject tempObject = null;
            if (objects.Length > 1)
            {
                float dist = 0;
                dist = Vector3.Distance(gameObject.transform.position, objects[0].transform.position);
                tempObject = objects[0];
                for (var i = 1; i < objects.Length; i++)
                {
                    if (Vector3.Distance(gameObject.transform.position, objects[i].transform.position) < dist)
                    {
                        dist = Vector3.Distance(gameObject.transform.position, objects[i].transform.position);
                        tempObject = objects[i];
                    }
                }
            }
            else
                return null;

            return tempObject.GetComponent<Waypoint>();
        }

        public static float CalculateThreePointAngle(Vector3 first, Vector3 second, Vector3 third)
        {
            var threePointAngle = Vector2.Angle(
                new Vector2(first.x, first.z) - new Vector2(second.x, second.z),
                new Vector2(third.x, third.z) - new Vector2(second.x, second.z));

            return threePointAngle;
        }
        
        public static float CalculateThreePointSignedAngle(Vector3 first, Vector3 second, Vector3 third)
        {
            var threePointAngle = Vector2.SignedAngle(
                new Vector2(first.x, first.z) - new Vector2(second.x, second.z),
                new Vector2(third.x, third.z) - new Vector2(second.x, second.z));

            return threePointAngle;
        }
    }
}