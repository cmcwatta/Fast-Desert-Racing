using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionDelay : MonoBehaviour
{
    public Action OnDestroyExplosion;
    void Start()
    {
        StartCoroutine(DelayDestroy());
    }

    private IEnumerator DelayDestroy()
    {
        yield return new WaitForSeconds(3f);
        if(gameObject != null && gameObject.activeSelf)
        {
            OnDestroyExplosion?.Invoke();
            Destroy(gameObject);
        }
    }
}
