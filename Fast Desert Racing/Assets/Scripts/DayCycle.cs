using Alteruna;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayCycle : AttributesSync
{
    [Header("Day Cycle")]
    [SerializeField]
    private Transform sunTransform;
    [SerializeField]
    private float rate;

    private float _curWait;
    private void Update()
    {
        _curWait += Time.deltaTime;

        if (_curWait >= 1)
        {
            _curWait = 0;
            if (!Multiplayer.Instance.Me.IsHost) return;
            sunTransform.Rotate(new Vector3(rate, 0, 0));
            BroadcastRemoteMethod("SyncDay", sunTransform.rotation);
        }
    }

    [SynchronizableMethod]
    public void SyncDay(Quaternion rotation)
    {
        sunTransform.rotation = rotation;
    }
}
