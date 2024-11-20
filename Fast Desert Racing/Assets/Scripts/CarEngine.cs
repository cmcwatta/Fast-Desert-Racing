using Alteruna;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEngine : MonoBehaviour
{
    private Car _car;

    [SerializeField]
    private AudioSource engineAudio;

    [SerializeField]
    private AudioSource brakeAudio;
    void Start()
    {
        _car = GetComponentInParent<Car>();

        engineAudio.Play();
        brakeAudio.Play();
    }

    void Update()
    {
        if (_car.allowUse)
        {
            engineAudio.mute = false;
            engineAudio.pitch = Mathf.Clamp(_car.CurSpeed, 0, 1.5f);
            brakeAudio.mute = false;
            if (_car.wheelsCollider[0].WheelCollider.brakeTorque > 0)
            {
                brakeAudio.pitch = Mathf.Clamp(_car.CurSpeed, 0, 1f);
                brakeAudio.volume = 0.3f;
            }
            else
            {
                brakeAudio.volume = 0;
            }
        }
        else
        {
            engineAudio.mute = true;
            brakeAudio.mute = true;
        }
    }
}
