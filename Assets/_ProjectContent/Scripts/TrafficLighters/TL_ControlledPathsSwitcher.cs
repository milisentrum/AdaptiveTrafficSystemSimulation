using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdaptiveTrafficSystem.Paths;
using MyBox;
using UnityEngine;

namespace AdaptiveTrafficSystem.TrafficLighters
{
    public class TL_ControlledPathsSwitcher : MonoBehaviour
    {
        [SerializeField] private bool loadStartPaths;
        [SerializeField] [ConditionalField(nameof(loadStartPaths))] private List<ControlledPath> startPaths;
        
        private List<ControlledPath> _controlledPathsList;

        private void Start()
        {
            Init();
            StartCoroutine(Switching());
        }

        private void Init()
        {
            _controlledPathsList = loadStartPaths ? startPaths : new List<ControlledPath>();
        }

        private IEnumerator Switching()
        {
            if (_controlledPathsList.Count > 0)
            {
                while (true)
                {
                    foreach (var path in _controlledPathsList)
                    {
                        yield return WaitUntilSwitchToPath(path);
                        yield return new WaitForSeconds(path.TrafficGroup.OpenPathTime.GetValue());
                    }
                }
            }

            Debug.LogError("[TL_ControlledPathsSwitcher] No controlled paths");
            yield return null;
        }

        private IEnumerator WaitUntilSwitchToPath(ControlledPath path)
        {
            yield return CloseWithoutPath(path);
            yield return path.TrafficGroup.WaitUntilSwitchToOpen();
        }

        private IEnumerator CloseWithoutPath(ControlledPath openPath)
        {
            var pathsToClose = _controlledPathsList
                .Where(path => path != openPath)
                .ToArray();
            if (pathsToClose.Length > 0)
            {
                pathsToClose.ForEach(path => path.TrafficGroup.SwitchToClose());
            }

            yield return new WaitUntil(() => pathsToClose.All(path => path.TrafficGroup.IsAllClosed()));
        }
    }
}