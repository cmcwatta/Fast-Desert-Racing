using Alteruna;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Avatar = Alteruna.Avatar;
using Random = UnityEngine.Random;

public enum AIState
{
    Patrol,
    Follow
}

public class AI_Car : AttributesSync
{
    private NavMeshAgent _agent;

    [SerializeField]
    [SynchronizableField]
    private int _index = 0;

    [SynchronizableField]
    private string _followName = "";

    private Transform[] _transforms;

    [SerializeField]
    private float distanceCheck;
    [SerializeField]
    private float followSpeed;
    [SerializeField]
    private float patrolSpeed;
    [SerializeField]
    private Transform[] wheels;

    [SerializeField]
    [SynchronizableField]
    private AIState _curState;

    [SerializeField]
    public AudioSource hornAudio;


    [SerializeField]
    private AudioSource engineAudio;

    private bool _dead;
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
        if (Multiplayer.Me.IsHost)
        {
            switch (_curState)
            {
                case AIState.Patrol:
                    _agent.speed = patrolSpeed;
                    if (CheckDistance())
                    {
                        GoRandom();
                    }
                    break;
                case AIState.Follow:
                    _agent.speed = followSpeed;
                    Avatar avatar = FindObjectsOfType<Alteruna.Avatar>().FirstOrDefault(x => x.name == _followName);
                    if (avatar != null)
                    {
                        if (Vector3.Distance(avatar.transform.position, _agent.transform.position) < 100)
                        {
                            _agent?.SetDestination(avatar.transform.position);
                        }
                        else
                        {
                            _curState = AIState.Patrol;
                            Commit();
                        }
                    }
                    break;
                default:
                    _agent.SetDestination(_agent.transform.position);
                    GoRandom();
                    break;
            }
        }
        WheelsUpdate();
    }

    void GoRandom()
    {
        _index = Random.Range(0, _transforms.Length);
        Commit();
        _agent?.SetDestination(_transforms[_index].position);
    }

    void WheelsUpdate()
    {
        foreach(Transform wheel in wheels)
        {
            wheel.transform.Rotate(new Vector3(_agent.velocity.magnitude, 0, 0));
        }

        engineAudio.pitch = Mathf.Clamp(_agent.velocity.magnitude, 0, 1.5f);
    }

    bool CheckDistance()
    {
        if (Vector3.Distance(_transforms[_index].position, transform.position) < distanceCheck)
        {
            return true;
        }
        return false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (Multiplayer.Me.IsHost)
        {
            Transform carHit = collision.collider.gameObject.transform.parent;
            if (carHit.CompareTag("Player"))
            {
                BroadcastRemoteMethod("HornNPC", gameObject.GetInstanceID());
                _followName = carHit.name;
                _curState = AIState.Follow;
                Commit();
            }
        }
    }

    [SynchronizableMethod]
    public void HornNPC(int id)
    {
        GameObject car = GameObject.FindGameObjectsWithTag("NPC").FirstOrDefault(x => x.GetInstanceID() == id);
        if (car)
        {
            try
            {
                hornAudio.Play();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

    public void Death(string nameAttacker)
    {
        BroadcastRemoteMethod("SyncDead", transform.position, nameAttacker);
    }

    [SynchronizableMethod]
    public void SyncDead(Vector3 position, string nameAttacker)
    {
        AI_Car[] npcs = FindObjectsOfType<AI_Car>();
        foreach (AI_Car npc in npcs)
        {
            if (Vector3.Distance(npc.transform.position, position) < 1)
            {
                if (Multiplayer.Me.IsHost)
                {
                    _dead = true;
                    _agent.isStopped = true;

                    Spawner spawner = GameObject.Find("Multiplayer").GetComponent<Spawner>();
                    spawner.Spawn("Explosion", transform.position);
                    spawner.Despawn(gameObject);

                    RacingManager.OnScorePlayerAdd?.Invoke(nameAttacker, 1);
                }
                break;
            }
        }
    }
}
