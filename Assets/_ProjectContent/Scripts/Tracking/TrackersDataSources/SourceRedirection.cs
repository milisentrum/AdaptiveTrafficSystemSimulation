using System.Linq;
using UnityEngine;

namespace AdaptiveTrafficSystem.Tracking
{
    public class SourceRedirection : MonoBehaviour
    {
        [SerializeField] private TrackerDataSourceBase[] inputDataSources;
        [SerializeField] private TrackerDataSourceBase[] outputDataSources;

        private void Start()
        {
            Redirect();
        }

        private void Redirect()
        {
            foreach (var outputDataSource in outputDataSources)
            {
                outputDataSource.AddTrackingEvents(
                    inputDataSources.Select(inputDataSource => inputDataSource.OnDataTracked));
            }
        }
    }
}