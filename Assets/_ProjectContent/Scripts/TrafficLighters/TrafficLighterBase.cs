using System.Collections;
using UnityEngine;

namespace AdaptiveTrafficSystem.TrafficLighters
{
    public abstract class TrafficLighterBase : MonoBehaviour, ITrafficController
    {
        protected TrafficMode _trafficMode;

        private BoxCollider _collider;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
        }

        protected virtual void Start()
        {
            _trafficMode = TrafficMode.UNKNOWN;
        }
        
        protected abstract IEnumerator SwitchToGreen();

        protected abstract IEnumerator SwitchToRed();

        public void SwitchToOpen()
        {
            if (_trafficMode == TrafficMode.OPEN) return;
            StartCoroutine(SwitchToGreen());
        }

        public void SwitchToClose()
        {
            if (_trafficMode == TrafficMode.CLOSE) return;
            StartCoroutine(SwitchToRed());
        }

        public TrafficMode GetMode() => _trafficMode;

        public void SetColliderToRoad(float roadWidth)
        {
            var baseSize = _collider.size;
            _collider.size = new Vector3(roadWidth, baseSize.y, baseSize.z);

            var baseCenter = _collider.center;
            _collider.center = new Vector3(-roadWidth / 2, baseCenter.y, baseCenter.z);
        }
    }
}