using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MusicPlayer : MonoBehaviour
{
    [Serializable]
    public class MusicObject
    {
        public string Name;
        public AudioSource Source;
    }

    [SerializeField]
    private Slider timeline;
    [SerializeField]
    private TMP_Text startTime;
    [SerializeField]
    private TMP_Text endTime;
    [SerializeField]
    private TMP_Text songTitle;

    [SerializeField]
    private AudioClip[] songs;

    private List<MusicObject> _audioSources = new List<MusicObject>();

    private int _indexSong = 0;

    void Start()
    {
        foreach (var song in songs)
        {
            GameObject audioGameObject = new GameObject(song.name);
            audioGameObject.transform.parent = this.transform;
            AudioSource audioSource = audioGameObject.AddComponent<AudioSource>();
            audioSource.clip = song;
            audioSource.volume = 0.35f;
            _audioSources.Add(new MusicObject
            {
                Name = song.name,
                Source = audioSource
            });
        }
    }

    void Update()
    {
        float currentTime = _audioSources[_indexSong].Source.time;
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);

        startTime.text = $"{minutes}:{seconds:D2}";

        float timeEnd = _audioSources[_indexSong].Source.clip.length;
        int minutesEnd = Mathf.FloorToInt(timeEnd / 60);
        int secondsEnd = Mathf.FloorToInt(timeEnd % 60);

        endTime.text = $"{minutesEnd}:{secondsEnd:D2}";

        timeline.value = currentTime / timeEnd;
    }


    public void PlayPause()
    {
        foreach (var audios in _audioSources)
        {
            if (audios.Name == _audioSources[_indexSong].Name)
            {
                if (audios.Source.isPlaying)
                {
                    audios.Source.Pause();
                }
                else
                {
                    audios.Source.Play();
                }
            }
            else
            {
                audios.Source.Stop();
            }
        }

        songTitle.text = _audioSources[_indexSong].Name;
    }

    public void Next()
    {
        _indexSong = (_indexSong + 1) % _audioSources.Count;
        PlayPause();
    }

    public void Previous()
    {
        _indexSong = (_indexSong - 1 + _audioSources.Count) % _audioSources.Count;
        PlayPause();
    }
}
