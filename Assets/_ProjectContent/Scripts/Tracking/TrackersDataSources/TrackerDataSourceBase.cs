using System.Collections.Generic;
using AdaptiveTrafficSystem.Tracking.Filters;
using MyBox;
using UnityEngine;
using UnityEngine.Events;

namespace AdaptiveTrafficSystem.Tracking
{
    public abstract class TrackerDataSourceBase : MonoBehaviour, IDataStream
    {
        [SerializeField] protected TrackerBase[] trackers;
        [SerializeField] private bool gatherFromStart = true;
        public UnityEvent<GameObject> OnDataTracked;

        private IFilter[] _filters;
        private bool _isWorking;

        private void Start()
        {
            LoadFilters();
            AddTrackingEvents(GetDataInputEvent());
            if (gatherFromStart)
            {
                GatherData();
            }
        }

        private void LoadFilters()
        {
            _filters = GetComponents<IFilter>();
        }

        private GameObject ApplyFilters(GameObject trackedObject)
        {
            var filteringObject = trackedObject;

            foreach (var filter in _filters)
            {
                if (filteringObject != null)
                {
                    filteringObject = filter.Filter(filteringObject);
                }
            }

            return filteringObject;
        }

        public void GatherData()
        {
            _isWorking = true;
        }

        public void BlockData()
        {
            _isWorking = false;
        }

        public void AddTrackingEvents(IEnumerable<UnityEvent<GameObject>> trackingEvents)
        {
            foreach (var trackingEvent in trackingEvents)
            {
                trackingEvent.AddListener(HandleData);
            }
        }

        private void HandleData(GameObject trackedObject)
        {
            if (!_isWorking) return;

            var filteredObject = ApplyFilters(trackedObject);

            if (filteredObject != null)
            {
                OnDataTracked.Invoke(filteredObject);
            }
        }

        public abstract IEnumerable<UnityEvent<GameObject>> GetDataInputEvent();
        public UnityEvent<GameObject> GetDataOutputEvent() => OnDataTracked;
    }
}