using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEngine : MonoBehaviour
{
    private Car _car;

    [SerializeField]
    private AudioSource engineAudio;
    void Start()
    {
        _car = GetComponentInParent<Car>();

        engineAudio.Play();
    }

    void Update()
    {
        if (_car.allowUse)
        {
            engineAudio.mute = false;
            engineAudio.pitch = Mathf.Clamp(_car.CurSpeed, 0, 1.5f);
        }
        else
        {
            engineAudio.mute = true;
        }
    }
}
