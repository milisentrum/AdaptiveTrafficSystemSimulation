using System;
using System.Collections;
using System.Collections.Generic;
using AdaptiveTrafficSystem.Pedestrians.Data;
using AdaptiveTrafficSystem.Pedestrians.Modules;
using AdaptiveTrafficSystem.TrafficLighters;
using MyBox;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace AdaptiveTrafficSystem.Pedestrians
{
    public class PedestriansSpawningSystem : MonoBehaviour
    {
        [Separator("Spawning")]
        [SerializeField][DisplayInspector] private SpawningData spawningData;
        [SerializeField] private Transform holder;
        [SerializeField] private Transform[] spawningPoints;

        [Separator("Events")]
        [SerializeField] private UnityEvent<GameObject> onPedestrianSpawn;

        //void RegisterPedestrianAtCrossing(PedestrianNavigator ped, TrafficLighter crosswalkLighter)
        //{
        //    crosswalkLighter.OnSwitchedToRed.AddListener(ped.PauseAtRed);
        //    crosswalkLighter.OnSwitchedToGreen.AddListener(ped.ResumeOnGreen);
        //}
        private struct SpawnInfo
        {
            public GameObject[] Prototypes;
            public int Amount;
            public float StartDelay;
            public float Interval;
            public IReadOnlyList<Transform> Points;
        }

        private void Start()
        {
            Spawn();
        }

        public void Spawn(int? amount = null, float? startDelay = null, float? interval = null)
        {
            var spawnInfo = BuildSpawnInfo(amount, startDelay, interval);
            StartCoroutine(SpawnProcess(spawnInfo));
        }

        private SpawnInfo BuildSpawnInfo(int? amount = null, float? startDelay = null, float? interval = null)
        {
            var spawnInfo = new SpawnInfo
            {
                Prototypes = spawningData.Prototypes,
                Amount = amount ?? spawningData.Amount,
                StartDelay = startDelay ?? spawningData.StartDelay,
                Interval = interval ?? spawningData.Interval,
                Points = spawningPoints
            };

            if (!CheckSpawnParameters(spawnInfo))
            {
                throw new ArgumentException("[PedestriansSpawningSystem] Spawn parameters error");
            }

            return spawnInfo;
        }

        private IEnumerator SpawnProcess(SpawnInfo spawnInfo)
        {
            yield return new WaitForSeconds(spawnInfo.StartDelay);

            for (var i = 0; i < spawnInfo.Amount; i++)
            {
                var prototype = spawnInfo.Prototypes.GetRandom();
                var pedestrianObject = Instantiate(prototype, holder);

                var randomSpawnPosition = spawnInfo.Points.GetRandom().position;
                pedestrianObject.transform.position = randomSpawnPosition;
                pedestrianObject.GetComponentInChildren<NavMeshAgent>().Warp(randomSpawnPosition);
                pedestrianObject.SetActive(true);

                //if (pedestrianObject.GetComponent<AgentController>() == null)
                //    pedestrianObject.AddComponent<AgentController>();

                onPedestrianSpawn.Invoke(pedestrianObject);

                yield return new WaitForSeconds(spawnInfo.Interval);
            }
        }

        private static bool CheckSpawnParameters(SpawnInfo spawnInfo) =>
            spawnInfo.Prototypes.Length > 0 &&
            spawnInfo.Amount > 0 &&
            spawnInfo.StartDelay >= 0 &&
            spawnInfo.Interval > 0 &&
            spawnInfo.Points.Count > 0;
    }
}