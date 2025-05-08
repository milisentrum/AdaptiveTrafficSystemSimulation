using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using AdaptiveTrafficSystem.TrafficLighters;

public class PedestrianTriggerZone : MonoBehaviour
{
    [Tooltip("ID ����������� ���������, �������� 'p_na'")]
    public string lighterID;

    private TrafficLighter _trafficLighter;
    private Dictionary<NavMeshAgent, (UnityAction onGreen, UnityAction onRed)> _subscriptions;

    private void Start()
    {
        _subscriptions = new Dictionary<NavMeshAgent, (UnityAction, UnityAction)>();

        // ������� ������ TrafficLighter �� lighterID
        foreach (var tl in FindObjectsOfType<TrafficLighter>())
        {
            if (tl.LighterID == lighterID)
            {
                _trafficLighter = tl;
                break;
            }
        }
        if (_trafficLighter == null)
            Debug.LogError($"[PedestrianTriggerZone] �� ������ TrafficLighter � ID='{lighterID}'");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_trafficLighter == null || !other.CompareTag("Human"))
            return;

        // �������� NavMeshAgent � ��������
        var agent = other.GetComponentInChildren<NavMeshAgent>();
        if (agent == null)
            return;

        // ������ ��������-�����������, ����� ����� �� �������
        UnityAction onGreen = () => agent.isStopped = false;
        UnityAction onRed = () => agent.isStopped = true;

        // ������������� �� ������� ���������
        _trafficLighter.OnSwitchedToGreen.AddListener(onGreen);
        _trafficLighter.OnSwitchedToRed.AddListener(onRed);
        _subscriptions[agent] = (onGreen, onRed);

        // ����� ������ ��� ������� ���� � ����������� �� �������� ���������
        bool isClose = _trafficLighter.GetMode() == TrafficMode.CLOSE;
        agent.isStopped = isClose;

        Debug.Log($"[PedestrianTriggerZone] ����� {other.name}, ID={lighterID}, ���� = {agent.isStopped}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (_trafficLighter == null || !other.CompareTag("Human"))
            return;

        var agent = other.GetComponentInChildren<NavMeshAgent>();
        if (agent == null)
            return;

        // ������������ �� �������
        if (_subscriptions.TryGetValue(agent, out var subs))
        {
            _trafficLighter.OnSwitchedToGreen.RemoveListener(subs.onGreen);
            _trafficLighter.OnSwitchedToRed.RemoveListener(subs.onRed);
            _subscriptions.Remove(agent);
        }

        // ������� ����, ����� ������� ����� ����� ������
        agent.isStopped = false;
        Debug.Log($"[PedestrianTriggerZone] ������� {other.name}, ID={lighterID}");
    }
}
