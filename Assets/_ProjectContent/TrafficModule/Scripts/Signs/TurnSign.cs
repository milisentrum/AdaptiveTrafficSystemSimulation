using System;
using System.Collections.Generic;
using TrafficModule.Waypoints;
using UnityEngine;

namespace TrafficModule.Signs
{
    public class TurnSign : MonoBehaviour
    {
        public enum Type
        {
            STRAIGHT,
            LEFT = -140,
            RIGHT = 140,
            STRAIGHT_LEFT,
            STRAIGHT_RIGHT,
            LEFT_RIGHT,
        };

        public Type signType;

        public Waypoint waypointTarget;
        private readonly List<Waypoint> _removes = new List<Waypoint>();

        void Start()
        {
            var previous = waypointTarget.previous.transform.position;
            float threePointAngle = 0;

            foreach (var branch in waypointTarget.branches) // may be remove branches;
            {
                threePointAngle = Vector2.SignedAngle(
                    new Vector2(previous.x, previous.z) -
                    new Vector2(waypointTarget.transform.position.x, waypointTarget.transform.position.z),
                    new Vector2(branch.transform.position.x,
                        branch.transform.position.z) - new Vector2(waypointTarget.transform.position.x,
                        waypointTarget.transform.position.z));

                if (RemoveOrNot(threePointAngle)) _removes.Add(branch);
            }

            foreach (var branch in _removes) waypointTarget.branches.Remove(branch);

            threePointAngle = Vector2.SignedAngle( // may be remove waypoint next;
                new Vector2(previous.x, previous.z) -
                new Vector2(waypointTarget.transform.position.x, waypointTarget.transform.position.z),
                new Vector2(waypointTarget.next.transform.position.x,
                    waypointTarget.next.transform.position.z) - new Vector2(waypointTarget.transform.position.x,
                    waypointTarget.transform.position.z));

            if (RemoveOrNot(threePointAngle)) waypointTarget.RemoveNext();
        }

        private bool RemoveOrNot(float angle)
        {
            var ok = true;
            switch (signType)
            {
                case Type.STRAIGHT:
                    if (angle >= (int) Type.RIGHT || angle <= (int) Type.LEFT) ok = false;
                    break;
                case Type.LEFT:
                    if (angle >= (int) Type.LEFT && angle < 0) ok = false;
                    break;
                case Type.RIGHT:
                    if (angle <= (int) Type.RIGHT && angle > 0) ok = false;
                    break;
                case Type.STRAIGHT_LEFT:
                    if (angle >= (int) Type.RIGHT || angle <= 0) ok = false;
                    break;
                case Type.STRAIGHT_RIGHT:
                    if (angle <= (int) Type.LEFT || angle >= 0) ok = false;
                    break;
                case Type.LEFT_RIGHT:
                    if ((angle <= (int) Type.RIGHT && angle > 0) || (angle >= (int) Type.LEFT && angle < 0)) ok = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return ok;
        }

    }
}