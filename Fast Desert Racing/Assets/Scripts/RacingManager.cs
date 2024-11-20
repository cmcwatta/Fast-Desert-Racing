using Alteruna;
using Alteruna.SyncedEvent;
using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class RacingManager : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera vCam;

    private Multiplayer _multiplayer;
    private bool _joined = false;
    [SerializeField]
    private GameObject connectingPanel;
    [SerializeField]
    private GameObject gamePanel;
    [SerializeField]
    private GameObject pausePanel;

    [SerializeField]
    private TMP_Text speedometerText;
    public static Action<float> OnSpeedUpdate;

    [SerializeField]
    private Transform[] spawnpoints;

    private void Awake()
    {
        _multiplayer = FindObjectOfType<Multiplayer>();

        _multiplayer.OnRoomListUpdated.AddListener((Multiplayer call) =>
        {
            if (_joined) return;
            if (!call.AvailableRooms.Any(r => r.Name == "MainServer") || call.AvailableRooms.Count <= 0)
            {
                _multiplayer.CreateRoom("MainServer", false, 0, false, false);
            }
            else
            {
                foreach (var ro in call.AvailableRooms)
                {
                    Debug.Log(ro.Name);
                    if (ro.Name == "MainServer")
                    {
                        _multiplayer.JoinRoom(ro);
                        break;
                    }
                }
            }
        });

        _multiplayer.OnConnected.AddListener((Multiplayer a, Endpoint b) =>
        {
            if (!_joined) a.RefreshRoomList();
        });

        _multiplayer.OnRoomJoined.AddListener(JoinedRoom);

        OnSpeedUpdate += delegate (float speed)
        {
            speedometerText.text = Math.Round(speed * 2).ToString("0") + " KM/S";
        };
    }


    private void JoinedRoom(Multiplayer multiplayer, Room room, User user)
    {
        Spawner spawner = GameObject.Find("Multiplayer").GetComponent<Spawner>();
        Car car = spawner.Spawn(PlayerPrefs.GetString("Model"), transform.position).GetComponent<Car>();
        car.GetComponent<Alteruna.Avatar>().Possessed(user);
        car.transform.position = spawnpoints[Random.Range(0, spawnpoints.Length)].position;
        _joined = true;
    }

    private void Update()
    {
        connectingPanel.SetActive(!_joined);
        gamePanel.SetActive(_joined);

        Alteruna.Avatar[] avatars = FindObjectsOfType<Alteruna.Avatar>();

        foreach (Alteruna.Avatar avatar in avatars.Where(x => x.GetComponent<AI_Car>() == null))
        {
            if (avatar.IsMe)
            {
                vCam.LookAt = avatar.transform;
                vCam.Follow = avatar.transform;
                avatar.GetComponent<Car>().allowUse = true;
            }
        }

        if (_joined && Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenu();
        }
    }

    public void PauseMenu()
    {
        pausePanel.SetActive(!pausePanel.activeSelf);
    }

    public void BackMainMenu()
    {
        SceneManager.LoadScene(0);
    }
    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
