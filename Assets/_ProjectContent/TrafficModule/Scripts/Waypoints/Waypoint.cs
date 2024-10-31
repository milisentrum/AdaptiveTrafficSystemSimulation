using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using TrafficModule.Vehicle.Extensions;
using UnityDevKit.Optimization;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TrafficModule.Waypoints
{
// Transport move on this point. Take position, next point and branches. Also control transport speed on turns.
    public class Waypoint : CachedMonoBehaviour
    {
        [Separator("Settings", true)]
        [Range(0f, 5f)] public float width;
        public bool handInit;
        public bool intersection;
        
        [Separator("Path", true)]
        public Waypoint previous;
        public Waypoint next;

        [Separator("Branches", true)]
        [Range(0f, 1f)] public float branchRatio = 0.5f;
        public List<Waypoint> branches;
        public List<Waypoint> previousBranches;
        public List<Waypoint>  laneChangeBranches;

        private readonly List<Waypoint> _allPrevious = new();
        private readonly List<Waypoint> _allFuture = new();
        private readonly Dictionary<(Waypoint, Waypoint), BranchData> nextWaypointData = new();

        private List<DebugBranchData> _branchDatas = new List<DebugBranchData>();

        [Serializable]
        public struct Branch
        {
            public Waypoint Waypoint;
            public VehiclePriority.PriorityType PriorityType;
        }

        [Serializable]
        public struct BranchData
        {
            public float Angle;
            public float SignedAngle;
            public int Speed;
            public VehiclePriority.PriorityType PriorityType;
        }
        
        [Serializable]
        public struct DebugBranchData
        {
            public Waypoint PreviousWaypoint;
            public Waypoint NextWaypoint;
            public BranchData BranchData;
        }

        private void Start()
        {
            if (handInit)
            {
                Init();
            }
        }

        //calculate data for all combination of points
        private void SetWaypointsData()
        {
            //iterate through all
            if (_allFuture.Count == 0 || _allPrevious.Count == 0) return;
            foreach (var nextWaypoint in _allFuture)
            {
                foreach (var previousWaypoint in _allPrevious)
                {
                    // calculate angle depends on speed
                    var angle =
                        Utils.Utils.CalculateThreePointAngle(
                            previousWaypoint.SelfPosition,
                            SelfPosition,
                            nextWaypoint.SelfPosition);
                    var signedAngle = Utils.Utils.CalculateThreePointSignedAngle(
                        previousWaypoint.SelfPosition,
                        SelfPosition,
                        nextWaypoint.SelfPosition);
                    
                    if (!nextWaypointData.ContainsKey((previousWaypoint, nextWaypoint)))
                    {
                        var speed = CalculateSpeedOnAngle(angle);
                        var priority = CalculatePriority(signedAngle);
                            
                        var branchData = new BranchData
                        {
                            Angle = angle,
                            SignedAngle = signedAngle,
                            Speed = speed,
                            PriorityType = priority
                        };
                        nextWaypointData.Add((previousWaypoint, nextWaypoint), branchData);
                        _branchDatas.Add(new DebugBranchData
                        {
                            PreviousWaypoint = previousWaypoint, NextWaypoint = nextWaypoint,
                            BranchData = branchData
                        });
                    }
                }
            }
        }

        private static VehiclePriority.PriorityType CalculatePriority(float angle) // TODO -- relative angles
        {
            var priority = angle switch
            {
                >= -180 and <= -165 => VehiclePriority.PriorityType.StraightCross,
                > -165 and <= -95 => VehiclePriority.PriorityType.LeftCross,
                > -95 and <= 0 => VehiclePriority.PriorityType.BackCross,
                > 0 and <= 95 => VehiclePriority.PriorityType.BackCross,
                > 95 and <= 165 => VehiclePriority.PriorityType.RightCross,
                > 165 and <= 180 => VehiclePriority.PriorityType.StraightCross,
                _ => VehiclePriority.PriorityType.Default
            };
            return priority;
        }

        // return speed value depends on angle
        private static int CalculateSpeedOnAngle(float angle) // TODO -- ratio from max speed
        {
            var angleSpeed = angle switch
            {
                >= 0 and <= 40 => 10,
                > 40 and <= 70 => 15,
                > 70 and <= 100 => 20,
                > 100 and <= 130 => 25,
                > 130 and <= 145 => 30,
                > 145 and <= 160 => 35,
                > 160 and <= 165 => 45,
                _ => 0
            };

            return angleSpeed;
        }

        private void InitBranches()
        {
            if (previous != null)
            {
                _allPrevious.Add(previous);
            }

            _allPrevious.AddRange(previousBranches);

            if (next != null)
            {
                _allFuture.Add(next);
            }

            _allFuture.AddRange(branches);
            _allFuture.AddRange(laneChangeBranches);
        }

        public void Init()
        {
            if (next == null && branches.Count > 0)
            {
                branchRatio = 1f;
            }

            InitBranches();
            SetWaypointsData();
        }

        // return Vector3 in some range near point
        public Vector3 GetPosition()
        {
            var position = SelfPosition;
            var right = CachedTransform.right;
            var minBound = position + right * width / 2f;
            var maxBound = position - right * width / 2f;

            return Vector3.Lerp(minBound, maxBound, Random.Range(0f, 1f));
        }


        // method for TurnSign
        public void RemoveNext()
        {
            var currentAngle = 180f;
            if (previous != null && branches != null)
            {
                foreach (var branch in branches)
                {
                    // delete current next and chose next point with the smallest Angle from available points
                    var threePointAngle = Vector2.Angle(
                        new Vector2(previous.SelfPosition.x, previous.SelfPosition.z)
                        - new Vector2(SelfPosition.x, SelfPosition.z),
                        new Vector2(branch.SelfPosition.x, branch.SelfPosition.z)
                        - new Vector2(SelfPosition.x, SelfPosition.z));
                    if (threePointAngle < currentAngle)
                    {
                        currentAngle = threePointAngle;
                        next.previous = null;
                        next = branch;
                    }
                }
            }
            else
            {
                // or just delete if there is no other points
                next.previous = null;
                next = null;
            }
        }

        // return speed value from dict on 2 points
        public int GetWaypointSpeed(Waypoint from, Waypoint to)
        {
            var speed = 0;
            if (nextWaypointData.ContainsKey((from, to)))
            {
                speed = nextWaypointData[(from, to)].Speed;
            }

            return speed;
        }

        public VehiclePriority.PriorityType GetWaypointPriorityType(Waypoint from, Waypoint to)
        {
            return nextWaypointData.ContainsKey((from, to))
                ? nextWaypointData[(from, to)].PriorityType
                : VehiclePriority.PriorityType.Default;
        }
        
        public BranchData? GetWaypointBranchData(Waypoint from, Waypoint to)
        {
            return nextWaypointData.ContainsKey((from, to)) ? nextWaypointData[(from, to)] : null;
        }

        // On delete
        public void RemoveFromAllPrevious(Waypoint waypoint)
        {
            _allPrevious.Remove(waypoint);
        }

        // On delete
        public void RemoveFromAllFuture(Waypoint waypoint)
        {
            _allFuture.Remove(waypoint);
        }

        public bool HasLaneChangeBranches => laneChangeBranches.Count > 0;
        
        private void OnDrawGizmos()
        {
            const float radius = 0.5f; 
            var tr = transform;
            var pos = tr.position;

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(pos, radius);
            
            Gizmos.color = Color.white;
            var right = tr.right;
            Gizmos.DrawLine(
                pos + (right * width / 2f),
                pos - (right * width / 2f));

            if (previous != null)
            {
                Gizmos.color = Color.red;
                var offset = tr.right * width / 2f;
                var previousTransform = previous.transform;
                var offsetTo = previousTransform.right * previous.width / 2f;
                Gizmos.DrawLine(pos + offset, previousTransform.position + offsetTo);
            }

            if (next != null)
            {
                Gizmos.color = Color.green;
                var offset = tr.right * -width / 2f;
                var nextTransform = next.transform;
                var offsetTo = nextTransform.right * -next.width / 2f;
                Gizmos.DrawLine(pos + offset, nextTransform.position + offsetTo);
            }

            if (branches != null)
            {
                foreach (var branch in branches.Where(branch => branch != null))
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(pos, branch.transform.position);
                }
            }
            
            if (laneChangeBranches != null)
            {
                foreach (var branch in laneChangeBranches.Where(branch => branch != null))
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(pos, branch.transform.position);
                }
            }
        }
    }
}