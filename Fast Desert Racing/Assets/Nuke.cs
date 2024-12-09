using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Nuke : MonoBehaviour
{
    public static Action OnExplode;

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(DestroyCoroutine());
    }

    private IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(1.5f);
        OnExplode?.Invoke();
        Destroy(gameObject);
    }
}
