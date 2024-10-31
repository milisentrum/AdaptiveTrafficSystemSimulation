using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AdaptiveTrafficSystem.Tracking
{
    public class ColliderDetector : DetectorBase
    {
        public List<Collider> Colliders { get; private set; } = new List<Collider>();
        
        private void Awake()
        {
            Colliders = GetComponents<Collider>().ToList();
        }

        public void TurnOnColliders()
        {
            foreach (var col in Colliders)
            {
                col.enabled = true;
            }
        }
        
        public void TurnOffColliders()
        {
            foreach (var col in Colliders)
            {
                col.enabled = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Detect(other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            Lose(other.gameObject);
        }
    }
}