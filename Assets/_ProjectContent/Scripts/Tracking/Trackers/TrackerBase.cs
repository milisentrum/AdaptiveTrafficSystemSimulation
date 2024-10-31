using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.Events;

namespace AdaptiveTrafficSystem.Tracking
{
    public abstract class TrackerBase : MonoBehaviour, ITracker
    {
        [SerializeField] private List<DetectorBase> detectors;

        [Foldout("Detectors setup", true)] [SerializeField]
        private bool setupLayerMask;

        [SerializeField] [ConditionalField(nameof(setupLayerMask))]
        private LayerMask layerMask;

        [SerializeField] private bool setupAllowedTags;

        [SerializeField] [ConditionalField(nameof(setupAllowedTags))]
        private List<string> allowedTags;

        /*  TODO -- detectors setup
            TODO -- layers/tags filtering   
        */

        public UnityEvent<GameObject> OnDetectEvent ;
        public UnityEvent<GameObject> OnLoseEvent;

        private void Start()
        {
            SetupDetectors();
            AddDetectSubscriptions();
            AddLoseSubscription();
        }

        public void AddDetector(DetectorBase detector)
        {
            detectors.Add(detector);
            SetupDetector(detector);
            detector.SubscribeToDetect(Detect);
        }

        public void RemoveDetector(DetectorBase detector)
        {
            if (!detectors.Contains(detector)) return;

            detector.UnsubscribeToDetect(Detect);
            detector.UnsubscribeToLose(Lose);
            detectors.Remove(detector);
        }

        public void RemoveAllDetectors()
        {
            for (var i = detectors.Count - 1; i >= 0 ; i++)
            {
                var detector = detectors[i];
                RemoveDetector(detector);
            }
        }

        private void SetupDetectors()
        {
            if (!setupLayerMask && !setupAllowedTags) return;
            foreach (var detector in detectors)
            {
                SetupDetector(detector);
            }
        }

        private void SetupDetector(DetectorBase detector)
        {
            if (setupLayerMask)
            {
                detector.SetLayerMask(layerMask);
            }

            if (setupAllowedTags)
            {
                detector.SetAllowedTags(allowedTags);
            }
        }

        private void AddDetectSubscriptions()
        {
            foreach (var detector in detectors)
            {
                detector.SubscribeToDetect(Detect);
            }
        }

        private void RemoveDetectSubscriptions()
        {
            foreach (var detector in detectors)
            {
                detector.UnsubscribeToDetect(Detect);
            }
        }

        private void AddLoseSubscription()
        {
            foreach (var detector in detectors)
            {
                detector.SubscribeToLose(Lose);
            }
        }

        private void RemoveLoseSubscriptions()
        {
            foreach (var detector in detectors)
            {
                detector.UnsubscribeToLose(Lose);
            }
        }

        private void Detect(GameObject detectedObject)
        {
            if (!ConfirmDetection(detectedObject)) return;
            OnDetectEvent.Invoke(detectedObject);
        }

        private void Lose(GameObject detectedObject)
        {
            OnLoseEvent.Invoke(detectedObject);
        }

        protected virtual bool ConfirmDetection(GameObject detectedObject) => true;
    }
}