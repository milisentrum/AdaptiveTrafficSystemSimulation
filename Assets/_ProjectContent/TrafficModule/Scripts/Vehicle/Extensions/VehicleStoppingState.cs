using System;
using MyBox;
using UnityEngine;

namespace TrafficModule.Vehicle.Extensions
{
    [RequireComponent(typeof(VehicleController))]
    public class VehicleStoppingState : MonoBehaviour, IVehicleExtension
    {
        [ReadOnly] public StoppingInfo _stoppingInfo = new StoppingInfo();
        [ReadOnly] public bool IsInJam;

        [Serializable]
        public class StoppingInfo
        {
            public bool NeedStopByVehicle;
            public bool NeedStopByTrafficLight;
            public bool NeedStopByCrossing;
            public bool NeedStopByPedestrian;
            public bool HasHighPriority;

            public bool NeedStop => NeedStopByVehicle ||
                                    NeedStopByTrafficLight ||
                                    NeedStopByCrossing ||
                                    NeedStopByPedestrian;

            public bool NeedExtremeStop => NeedStopByVehicle && !HasHighPriority ||
                                           NeedStopByTrafficLight ||
                                           NeedStopByPedestrian;

            public void Reset()
            {
                NeedStopByVehicle = false;
                NeedStopByTrafficLight = false;
                NeedStopByCrossing = false;
                NeedStopByPedestrian = false;
                //HasHighPriority = false;
            }
        }

        public void Init()
        {
        }

        private void Update()
        {
            const int frameFilter = 2;
            if (Time.frameCount % frameFilter == 0)
            {
                _stoppingInfo.Reset();
            }
        }
    }
}