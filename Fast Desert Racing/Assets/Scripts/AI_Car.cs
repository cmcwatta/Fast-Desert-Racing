using Alteruna;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_Car : AttributesSync
{
    private NavMeshAgent _agent;
    
    [SynchronizableField]
    private int _index = 0;

    private Transform[] _transforms;

    [SerializeField]
    private float distanceCheck;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();

        _transforms = GameObject.Find("Paths").GetComponentsInChildren<Transform>();

        if (!Multiplayer.Me.IsHost)
        {
            GetComponent<NavMeshAgent>().enabled = false;
        }
    }

    void Update()
    {
        if (CheckDistance())
        {
            if (Multiplayer.Me.IsHost)
            {
                _index = Random.Range(0, _transforms.Length);
                Commit();

                _agent?.SetDestination(_transforms[_index].position);
                BroadcastMessage("AgentDone", Multiplayer.Me.Name);
            }
        }
    }

    bool CheckDistance()
    {
        if (Vector3.Distance(_transforms[_index].position, transform.position) < distanceCheck)
        {
            return true;
        }
        return false;
    }

    [SynchronizableMethod]
    public void AgentDone(string name)
    {
        Debug.Log(name);
    }
}
