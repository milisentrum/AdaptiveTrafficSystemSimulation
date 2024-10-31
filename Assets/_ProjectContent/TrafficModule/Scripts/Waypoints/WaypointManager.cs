using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityDevKit.Triggers;
using UnityEngine;

namespace TrafficModule.Waypoints
{
    public class WaypointManager : Singleton<WaypointManager>
    {
        [Separator("Initialization")]
        [SerializeField] private bool initWaypoints = true;
        [SerializeField] private bool needWaypointsGenerated;
        
        [Separator("Triggers")]
        [SerializeField] private BoolTriggerEvent waypointsInited;
        
        [Separator("Waypoints generation")]
        [SerializeField] private bool generatedWaypoints;
        [ConditionalField(nameof(generatedWaypoints))] [SerializeField] private BoolTriggerEvent waypointsGenerated;

        [ReadOnly] public List<Waypoint> allWaypoints;
        
        private void Start()
        {
            if (needWaypointsGenerated)
            {
                if (initWaypoints)
                {
                    InitSubscription();
                }
            }
            else
            {

                Init();
            }
        }

        private void InitSubscription()
        {
            waypointsGenerated.SubscribeToTrueValueSet(Init);
        }

        private void Init()
        {
            allWaypoints = FindObjectsOfType<Waypoint>().ToList();
            foreach (var waypoint in allWaypoints)
            {
                waypoint.Init();
            }
            
            waypointsInited.SetValue(true);
        }
    }
}