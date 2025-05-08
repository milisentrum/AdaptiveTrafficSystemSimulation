//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class AgentController : MonoBehaviour
//{
//    // Start is called before the first frame update
//    void Start()
//    {
        
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }
//}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using AdaptiveTrafficSystem.TrafficLighters;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentController : MonoBehaviour
{
    private NavMeshAgent _agent;
    private TrafficLighter _myLighter;

    void Awake()
    {
        // 1) ������� �������� NavMeshAgent � ���� ��� � �����
        _agent = GetComponent<NavMeshAgent>()
              ?? GetComponentInChildren<NavMeshAgent>();
        if (_agent == null)
        {
            Debug.LogError($"[{name}] AgentController: ��� NavMeshAgent!");
        }

        // 2) ����� ������� �����: ��������� � ���� TrafficLighter
        _myLighter = FindObjectsOfType<TrafficLighter>()
            .OrderBy(t => Vector3.Distance(t.transform.position, transform.position))
            .FirstOrDefault();
        if (_myLighter == null)
            Debug.LogError($"[{name}] AgentController: ��� �� ������ TrafficLighter � �����!");
    }

    void Update()
    {
        if (_agent == null || _myLighter == null) return;

        // 3) ������� �����: ���� ������� � ����, ����� �������
        bool isRed = _myLighter.GetMode() == TrafficMode.CLOSE;
        if (_agent.isStopped != isRed)
            _agent.isStopped = isRed;
    }
}

