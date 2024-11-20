using Alteruna;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Avatar = Alteruna.Avatar;

public class NPCsHandler : AttributesSync
{
    [SerializeField]
    private GameObject[] npcs;
    [SerializeField]
    private Transform spawnPoint;

    [SerializeField]
    private float delay;
    private float _curDelay;
    [SerializeField]
    private float maxNpcs;

    void Update()
    {
        _curDelay += Time.deltaTime;

        if (_curDelay > delay && Multiplayer.Instance.Me.IsHost && ShouldSpawn())
        {
            _curDelay = 0;
            SpawnCar();
        }
    }

    void SpawnCar()
    {
        Spawner spawner = GameObject.Find("Multiplayer").GetComponent<Spawner>();
        GameObject npc = spawner.Spawn(npcs[Random.Range(0, npcs.Length)].name, transform.position);
        npc.GetComponent<Avatar>().Possessed(Multiplayer.Instance.Me);
    }

    bool ShouldSpawn()
    {
        return GameObject.FindObjectsOfType(typeof(AI_Car)).Length < maxNpcs;
    }
}
