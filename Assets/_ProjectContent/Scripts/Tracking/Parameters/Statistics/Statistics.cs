using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MyBox;
using UnityEngine;

namespace AdaptiveTrafficSystem.Tracking.Parameters.Statistics
{
    public abstract class Statistics<TParameter> : MonoBehaviour
        where TParameter : ITrackingParameter
    {
        [SerializeField] private TParameter[] parameters;
        [SerializeField] [InitializationField] private float refreshPeriod = 5;
        [SerializeField] [InitializationField] private float duration = 180;

        private int _iterationsCount;
        private List<float> _data;

        private sealed class StatsData
        {
            public string Name;
            public float Value;
            public float Duration;
            public float RefreshPeriod;
        }
        
        private void Start()
        {
            _iterationsCount = Mathf.CeilToInt(duration / refreshPeriod);
            _data = new List<float>(_iterationsCount);

            StartCoroutine(CollectData());
        }

        private IEnumerator CollectData()
        {
            yield return new WaitForSeconds(refreshPeriod);
            for (var i = 0; i < _iterationsCount; i++)
            {
                _data.Add(parameters.Sum(parameter => parameter.GetValue()));
                yield return new WaitForSeconds(refreshPeriod);
            }

            Save();
        }

        private void Save()
        {
            var resultValue = ProcessData(_data);
            var resultMsg =
                $"Statistics for {parameters[0].GetName()}: {resultValue} [duration={duration}; refreshPeriod={refreshPeriod}]";
            Debug.Log(resultMsg);

            var statsData = new StatsData
            {
                Name = parameters[0].GetName(),
                Value = resultValue,
                Duration = duration,
                RefreshPeriod = refreshPeriod
            };
            
            SaveToJson(statsData);
        }

        private static void SaveToJson(StatsData statsData)
        {
            const string statsPath = "Statistics";
            var path = Application.streamingAssetsPath + $"/{statsPath}";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            
            var dateTimeString = DateTime.Now.ToString("yyyy_MM_dd_H_mm");
            var fullPath = $"{path}/{dateTimeString}_{statsData.Name.Replace(' ', '_')}_stats.json";

            var json = JsonUtility.ToJson(statsData);
            File.WriteAllText(fullPath, json);
        }

        protected abstract float ProcessData(IEnumerable<float> data);
    }
}