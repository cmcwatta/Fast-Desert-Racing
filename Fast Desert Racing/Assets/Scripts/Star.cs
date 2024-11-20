using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Star: MonoBehaviour
{
    public static Action Collected;

    public AudioSource audioFx;

    private void OnTriggerEnter(Collider other)
    {
        Collected?.Invoke();

        audioFx.Play();

        gameObject.transform.localScale = new Vector3(0, 0, 0);

        StartCoroutine(HandleStopCoroutine(audioFx.clip.length));
    }

    private IEnumerator HandleStopCoroutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        Destroy(gameObject);
    }
}
