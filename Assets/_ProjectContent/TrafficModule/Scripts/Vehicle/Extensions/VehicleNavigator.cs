#define DEBUG_DRAW

using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using TrafficModule.Waypoints;
using UnityDevKit.Events;
using UnityDevKit.Optimization;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TrafficModule.Vehicle.Extensions
{
//This class is used to find future points and reset links on Waypoints
    [RequireComponent(typeof(VehicleController))]
    public class VehicleNavigator : CachedMonoBehaviour, IVehicleExtension
    {
        [SerializeField] private bool drawNavigation = true;
        [SerializeField] private LaneChangeData laneChangeData = new LaneChangeData {Changes = 2, Ratio = 0.25f};
        
        public Transform frontPoint;
        public Path currentPath;
        [ReadOnly] public Waypoint lastFindNextUpdate;
        
        public EventHolderBase OnDestinationReached { get; private set; } = new EventHolderBase();
        public EventHolderBase OnLeftLaneChangeStart { get; private set; } = new EventHolderBase();
        public EventHolderBase OnLeftLaneChangeComplete { get; private set; } = new EventHolderBase();
        public EventHolderBase OnRightLaneChangeStart { get; private set; } = new EventHolderBase();
        public EventHolderBase OnRightChangeComplete { get; private set; } = new EventHolderBase();

        private VehicleController _controller;
        private VehicleMovement _movement;

        private Vector3 _destination = new Vector3(0, 0);
        private bool _destReached = false;
        private float _allFuturePointsLength;

        private Queue<Waypoint> _laneChangeWaypoints = new Queue<Waypoint>();
        
        private bool _willBeDestroyed;
        
        // DESTROY
        private const float DESTROY_DELAY = 5f;

        [Serializable]
        public class LaneChangeData
        {
            public int Changes;
            public float Ratio;
        }

        private void Start()
        {
            _controller = GetComponent<VehicleController>();
            _movement = _controller.Movement;
        }

        private void Update()
        {
            if (!currentPath.IsInited || currentPath.CurrentWaypoint == null)
            {
                return;
            }

            var frontPointPosition = frontPoint.position;

            // if we almost reached the waypoint
            if (Vector3.Distance(frontPointPosition, _destination) < _movement.CalculateCloseView() ||
                Vector3.Distance(SelfPosition, _destination) <
                Vector3.Distance(frontPointPosition, SelfPosition))
            {
                _destReached = true;
                DestinationReached();
            }

            var rayLength = Vector3.Distance(
                frontPointPosition,
                frontPointPosition + frontPoint.forward * _movement.CalculateDistantView());

            if (currentPath.FuturePoints.Count > 0)
            {
                var currentWaypointPosition = currentPath.CurrentWaypoint.SelfPosition;
                _allFuturePointsLength =
                    Vector3.Distance(frontPointPosition, currentWaypointPosition) +
                    Vector3.Distance(currentWaypointPosition, currentPath.FuturePoints[0].SelfPosition);

                for (var i = 0; i < currentPath.FuturePoints.Count - 1; i++)
                {
                    _allFuturePointsLength += Vector3.Distance(currentPath.FuturePoints[i].SelfPosition,
                        currentPath.FuturePoints[i + 1].SelfPosition);
                }

                if (rayLength > _allFuturePointsLength && currentPath.FuturePoints.Count > 1)
                {
                    AddOneMorePoint();
                    _allFuturePointsLength += Vector3.Distance(
                        currentPath.FuturePoints[^2].SelfPosition,
                        currentPath.FuturePoints[^1].SelfPosition);
                }
            }

            if (!currentPath.HasPath && !_willBeDestroyed)
            {
                _willBeDestroyed = true;
                Invoke(nameof(DestroyTransport), DESTROY_DELAY);
            }
        }

        public void Init()
        {
            if (currentPath.CurrentWaypoint != null)
            {
                _destReached = false;
                currentPath.PreviousWaypoint = currentPath.CurrentWaypoint.previous != null
                    ? currentPath.CurrentWaypoint.previous
                    : null;
                currentPath.FutureWaypoint = FindFutureWaypoint(currentPath.CurrentWaypoint);
                var point = FindFutureWaypoint(currentPath.CurrentWaypoint);
                if (point != null)
                {
                    currentPath.FuturePoints.Add(FindFutureWaypoint(currentPath.CurrentWaypoint));
                }

                _destination = currentPath.CurrentWaypoint.SelfPosition;
                currentPath.IsInited = true;
            }
        }


        // this method is called when we Reached the currentPath.currentWaypoint and need to update 
        private void DestinationReached()
        {
            if (_destReached)
            {
                // current -> prev, future -> current + need to find new one
                var oldPreviousWaypoint = currentPath.PreviousWaypoint;
                currentPath.PreviousWaypoint = currentPath.CurrentWaypoint;
                currentPath.CurrentWaypoint = currentPath.FutureWaypoint;

                if (currentPath.FuturePoints.Count > 0)
                {
                    currentPath.CurrentWaypoint = currentPath.FuturePoints[0];
                    currentPath.FuturePoints.RemoveAt(0);
                    _destination = currentPath.CurrentWaypoint.SelfPosition;
                }

                if (currentPath.CurrentWaypoint != null)
                {
                    if (_laneChangeWaypoints.Count > 0)
                    {
                        var data = currentPath.PreviousWaypoint.GetWaypointBranchData(oldPreviousWaypoint,
                            currentPath.CurrentWaypoint);

                        if (data != null)
                        {
                            var isLeftChange = ((Waypoint.BranchData) data).SignedAngle < 0;
                            
                            if (currentPath.CurrentWaypoint == _laneChangeWaypoints.Peek())
                            {
                                if (isLeftChange)
                                {
                                    OnLeftLaneChangeStart.Invoke();
                                }
                                else
                                {
                                    OnRightLaneChangeStart.Invoke();
                                }
                            }
                            else if (currentPath.PreviousWaypoint == _laneChangeWaypoints.Peek())
                            {
                                _laneChangeWaypoints.Dequeue();
                                if (isLeftChange)
                                {
                                    OnLeftLaneChangeComplete.Invoke();
                                }
                                else
                                {
                                    OnRightChangeComplete.Invoke();
                                }
                            }
                        }
                    }

                    var point = FindFutureWaypoint(currentPath.CurrentWaypoint);
                    if (currentPath.FuturePoints.Count == 0 && lastFindNextUpdate != currentPath.CurrentWaypoint)
                    {
                        if (point != null)
                        {
                            currentPath.FuturePoints.Add(point);
                        }

                        lastFindNextUpdate = currentPath.CurrentWaypoint;
                    }

                    currentPath.FutureWaypoint = point;
                    _destReached = false;
                }
                else
                {
                    currentPath.FuturePoints.Clear();
                    currentPath.FutureWaypoint = null;
                }

                OnDestinationReached.Invoke();
            }
        }

        private void AddOneMorePoint()
        {
            if (currentPath.FuturePoints.Count > 0 && lastFindNextUpdate != currentPath.FuturePoints.Last())
            {
                var point = FindFutureWaypoint(currentPath.FuturePoints.Last());
                if (point != null)
                {
                    currentPath.FuturePoints.Add(point);
                    lastFindNextUpdate = currentPath.FuturePoints[^2];
                }
                else
                {
                    lastFindNextUpdate = currentPath.FuturePoints[^1];
                }
            }
        }

        private Waypoint FindFutureWaypoint(Waypoint waypoint)
        {
            Waypoint resultWaypoint;

            var shouldBranch = false;
            if (waypoint.branches is {Count: > 0})
            {
                shouldBranch = Random.Range(0f, 1f) <= waypoint.branchRatio;
            }

            if (shouldBranch)
            {
                resultWaypoint = waypoint.branches.GetRandom();
            }
            else
            {
                if (waypoint.HasLaneChangeBranches &&
                    laneChangeData.Changes > 0 &&
                    Random.Range(0f, 1f) <= laneChangeData.Ratio)
                {
                    laneChangeData.Changes--;
                    laneChangeData.Ratio /= 2;
                    var rand = Random.Range(0, waypoint.laneChangeBranches.Count);
                    resultWaypoint = waypoint.laneChangeBranches[rand];
                    _laneChangeWaypoints.Enqueue(resultWaypoint);
                    return resultWaypoint;
                }

                resultWaypoint = waypoint.next;
            }

            return resultWaypoint;
        }
        
        private void DestroyTransport()
        {
            _controller.DestroyTransport();
        }

        [Serializable]
        public class Path
        {
            public Waypoint CurrentWaypoint { get; internal set; }
            public Waypoint PreviousWaypoint { get; internal set; }
            public Waypoint FutureWaypoint { get; internal set; }
            public List<Waypoint> FuturePoints { get; internal set; } = new List<Waypoint>();

            public bool IsInited { get; internal set; }

            public bool HasPath => CurrentWaypoint != null;

            public bool HasNoFuturePath => !HasPath || FuturePoints.Count <= 0;
        }

#if DEBUG_DRAW
        private void OnDrawGizmos()
        {
            if (!drawNavigation) return;
            const float navigationRadius = 2f;

            Gizmos.color = Color.green;
            if (currentPath.CurrentWaypoint != null)
            {
                Gizmos.DrawSphere(currentPath.CurrentWaypoint.SelfPosition, navigationRadius);
                Gizmos.DrawLine(SelfPosition, currentPath.CurrentWaypoint.SelfPosition);
            }

            Gizmos.color = Color.magenta;
            if (currentPath.FutureWaypoint != null)
            {
                Gizmos.DrawSphere(currentPath.FutureWaypoint.SelfPosition, navigationRadius);
                if (currentPath.CurrentWaypoint != null)
                {
                    Gizmos.DrawLine(currentPath.CurrentWaypoint.SelfPosition,
                        currentPath.FutureWaypoint.SelfPosition);
                }
            }

            Gizmos.color = Color.cyan;
            for (var i = 0; i < currentPath.FuturePoints.Count; i++)
            {
                Gizmos.DrawWireSphere(currentPath.FuturePoints[i].SelfPosition, navigationRadius / 2);
                if (i + 1 < currentPath.FuturePoints.Count)
                {
                    Gizmos.DrawLine(currentPath.FuturePoints[i].SelfPosition,
                        currentPath.FuturePoints[i + 1].SelfPosition);
                }
            }
        }
#endif
    }
}