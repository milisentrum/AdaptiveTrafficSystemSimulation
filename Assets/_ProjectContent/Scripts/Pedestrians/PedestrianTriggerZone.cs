using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using AdaptiveTrafficSystem.TrafficLighters;

public class PedestrianTriggerZone : MonoBehaviour
{
    [Tooltip("ID пешеходного светофора, например 'p_na'")]
    public string lighterID;

    private TrafficLighter _trafficLighter;
    private Dictionary<NavMeshAgent, (UnityAction onGreen, UnityAction onRed)> _subscriptions;

    private void Start()
    {
        _subscriptions = new Dictionary<NavMeshAgent, (UnityAction, UnityAction)>();

        // Находим нужный TrafficLighter по lighterID
        foreach (var tl in FindObjectsOfType<TrafficLighter>())
        {
            if (tl.LighterID == lighterID)
            {
                _trafficLighter = tl;
                break;
            }
        }
        if (_trafficLighter == null)
            Debug.LogError($"[PedestrianTriggerZone] Не найден TrafficLighter с ID='{lighterID}'");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_trafficLighter == null || !other.CompareTag("Human"))
            return;

        // Получаем NavMeshAgent у пешехода
        var agent = other.GetComponentInChildren<NavMeshAgent>();
        if (agent == null)
            return;

        // Создаём делегаты-обработчики, чтобы потом их удалить
        UnityAction onGreen = () => agent.isStopped = false;
        UnityAction onRed = () => agent.isStopped = true;

        // Подписываемся на события светофора
        _trafficLighter.OnSwitchedToGreen.AddListener(onGreen);
        _trafficLighter.OnSwitchedToRed.AddListener(onRed);
        _subscriptions[agent] = (onGreen, onRed);

        // Сразу ставим или снимаем стоп в зависимости от текущего состояния
        bool isClose = _trafficLighter.GetMode() == TrafficMode.CLOSE;
        agent.isStopped = isClose;

        Debug.Log($"[PedestrianTriggerZone] Вошёл {other.name}, ID={lighterID}, стоп = {agent.isStopped}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (_trafficLighter == null || !other.CompareTag("Human"))
            return;

        var agent = other.GetComponentInChildren<NavMeshAgent>();
        if (agent == null)
            return;

        // Отписываемся от событий
        if (_subscriptions.TryGetValue(agent, out var subs))
        {
            _trafficLighter.OnSwitchedToGreen.RemoveListener(subs.onGreen);
            _trafficLighter.OnSwitchedToRed.RemoveListener(subs.onRed);
            _subscriptions.Remove(agent);
        }

        // Снимаем стоп, чтобы пешеход точно пошёл дальше
        agent.isStopped = false;
        Debug.Log($"[PedestrianTriggerZone] Покинул {other.name}, ID={lighterID}");
    }
}
