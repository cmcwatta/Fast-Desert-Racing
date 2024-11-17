using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Sound
{
    [Range(0, 100)]
    public float Volume;

    public AudioClip AudioClip;

    public string Name;

    [HideInInspector]
    public AudioSource Source;
}

public class UISounds : MonoBehaviour
{
    public Sound[] Audios;

    private void Start()
    {
        foreach (var sound in Audios)
        {
            sound.Source = gameObject.AddComponent<AudioSource>();
            sound.Source.clip = sound.AudioClip;
            sound.Source.volume = sound.Volume / 100f;
            sound.Source.playOnAwake = false;
        }
    }

    public void Play(string soundName)
    {
        Sound sound = Array.Find(Audios, s => s.Name == soundName);
        if (sound != null)
        {
            sound.Source.Play();
        }
        else
        {
            Debug.LogWarning("Sound: " + soundName + " not found!");
        }
    }
}
