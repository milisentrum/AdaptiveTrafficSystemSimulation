using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace TrafficModule.Vehicle.Extensions
{
    [RequireComponent(typeof(VehicleController))]
    public class VehicleLighters : MonoBehaviour, IVehicleExtension
    {
        [Separator("Main lights")]
        [SerializeField] private List<Light> headlights = new List<Light>();
        [SerializeField] private List<Light> backlights = new List<Light>();
        
        [Separator("Settings")]
        [SerializeField] [Range(0f, 2f)] private float lowHeadlightsIntensity;
        [SerializeField] [Range(0, 150)] private int lowRange;
        [SerializeField] [Range(0f, 179f)] private float lowSpotAngle;
        [SerializeField] [Range(0f, 2f)] private float mainHeadlightsIntensity;
        [SerializeField] [Range(0, 150)] private int mainRange;
        [SerializeField] [Range(0f, 179f)] private float mainSpotAngle;
        [SerializeField] private bool mainBeam;
        [SerializeField] private bool useLights;

        [Separator("Turn lights")] 
        [SerializeField] private bool useLaneCrossLights = true;
        [SerializeField] private bool useCrossLights = true;
        [SerializeField] private GameObject[] leftLights;
        [SerializeField] private GameObject[] rightLights;

        private VehicleController _controller;
        private VehicleNavigator _navigator;
        private VehiclePriority _priority;

        private void Awake()
        {
            _controller = GetComponent<VehicleController>();
        }

        public void Init()
        {
            _navigator = _controller.Navigator;
            _priority = _controller.Priority;
            SubscribeToLaneChangeEvents();
            SubscribeToPriorityChange();
            SetSettings();

            TurnHeadlights(useLights);
            if (!useLights) return;

            foreach (var headlight in headlights)
            {
                headlight.intensity = mainBeam ? mainHeadlightsIntensity : lowHeadlightsIntensity;
                headlight.range = mainBeam ? mainRange : lowRange;
                headlight.spotAngle = mainBeam ? mainSpotAngle : lowSpotAngle;
            }
        }

        private void SubscribeToLaneChangeEvents()
        {
            if (!useLaneCrossLights) return;
            _navigator.OnLeftLaneChangeStart.AddListener(TurnOnLeftLights);
            _navigator.OnLeftLaneChangeComplete.AddListener(TurnOffLights);
            _navigator.OnRightLaneChangeStart.AddListener(TurnOnRightLights);
            _navigator.OnRightChangeComplete.AddListener(TurnOffLights);
        }

        private void SubscribeToPriorityChange()
        {
            if (!useCrossLights) return;
            _priority.OnPriorityChange.AddListener(PriorityHandler);
        }

        private void SetSettings()
        {
            var carSettings = _controller.vehicleData;
            useLights = carSettings.UseLights;
            lowHeadlightsIntensity = carSettings.LowHeadlightsIntensity;
            lowRange = carSettings.LowRange;
            lowSpotAngle = carSettings.LowSpotAngle;
            mainHeadlightsIntensity = carSettings.MainHeadlightsIntensity;
            mainRange = carSettings.MainRange;
            mainSpotAngle = carSettings.MainSpotAngle;
        }

        private void TurnOnLeftLights()
        {
            TurnOffRightLights();
            foreach (var leftLight in leftLights)
            {
                leftLight.SetActive(true);
            }
        }

        private void TurnOnRightLights()
        {
            TurnOffLeftLights();
            foreach (var rightLight in rightLights)
            {
                rightLight.SetActive(true);
            }
        }
        
        private void TurnOffLeftLights()
        {
            foreach (var leftLight in leftLights)
            {
                leftLight.SetActive(false);
            }
        }
        
        private void TurnOffRightLights()
        {
            foreach (var rightLight in rightLights)
            {
                rightLight.SetActive(false);
            }
        }

        private void TurnOffLights()
        {
            TurnOffLeftLights();
            TurnOffRightLights();
        }

        private void PriorityHandler(VehiclePriority.PriorityType priority)
        {
            TurnOffLights();
            switch (priority)
            {
                case VehiclePriority.PriorityType.LeftCross or VehiclePriority.PriorityType.BackCross:
                    TurnOnLeftLights();
                    break;
                case VehiclePriority.PriorityType.RightCross:
                    TurnOnRightLights();
                    break;
            }
        }

        public void TurnHeadlights(bool state)
        {
            foreach (var headlight in headlights)
            {
                headlight.enabled = state;
            }
        }

        public void ChangeBeam()
        {
            mainBeam = !mainBeam;
            foreach (var headlight in headlights)
            {
                headlight.intensity = mainBeam ? mainHeadlightsIntensity : lowHeadlightsIntensity;
                headlight.range = mainBeam ? mainRange : lowRange;
                headlight.spotAngle = mainBeam ? mainSpotAngle : lowSpotAngle;
            }
        }

        public void TurnBacklights(bool state)
        {
            foreach (var backlight in backlights)
            {
                backlight.enabled = state;
            }
        }
    }
}