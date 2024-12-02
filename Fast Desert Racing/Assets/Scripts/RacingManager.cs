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
    [SerializeField]
    private CinemachineVirtualCamera vFreeCam;
    [SerializeField]
    private GameObject cameraCenter;
    [SerializeField]
    private float mouseSensitivity = 100f;

    private Multiplayer _multiplayer;
    private bool _joined = false;
    [SerializeField]
    private GameObject connectingPanel;
    [SerializeField]
    private GameObject gamePanel;
    [SerializeField]
    private GameObject pausePanel;
    [SerializeField]
    private GameObject radioPanel;
    [SerializeField]
    private GameObject settingPanel;
    [SerializeField]
    private GameObject cameraMap;

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

        _multiplayer.OnOtherUserLeft.AddListener((Multiplayer _, User a) =>
        {
            foreach (Alteruna.Avatar avatar in FindObjectsOfType<Alteruna.Avatar>().Where(x => x.GetComponent<Car>() != null))
            {
                if (avatar.Possessor == a)
                {
                    Spawner spawner = GameObject.Find("Multiplayer").GetComponent<Spawner>();
                    spawner.Despawn(avatar.gameObject);
                }
            }
        });

        _multiplayer.OnRoomJoined.AddListener(JoinedRoom);

        OnSpeedUpdate += delegate (float speed)
        {
            speedometerText.text = Math.Round(speed * 2).ToString("0") + " KM/S";
        };
    }


    private void JoinedRoom(Multiplayer multiplayer, Room room, User user)
    {
        if (IsAlreadyJoined())
        {
            room.Leave();
            return;
        }

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

        foreach (Alteruna.Avatar avatar in avatars.Where(x => x.GetComponent<Car>() != null))
        {
            if (avatar.IsMe)
            {
                MoveCameraFreeMouse();
                if (Input.GetMouseButton(0))
                {
                    vCam.Priority = 5;
                    vFreeCam.Priority = 10;
                    vFreeCam.LookAt = null;
                    MoveCameraFreeMouse();
                }
                else
                {
                    vCam.LookAt = avatar.transform;
                    vCam.Follow = avatar.transform;
                    vFreeCam.LookAt = avatar.transform;
                    vCam.Priority = 10;
                    vFreeCam.Priority = 5;
                    vFreeCam.transform.position = vCam.transform.position;
                    vFreeCam.transform.rotation = vCam.transform.rotation;
                }

                cameraCenter.transform.position = avatar.transform.position;
                cameraMap.transform.position = new Vector3(avatar.transform.position.x, cameraMap.transform.position.y, avatar.transform.position.z);
                cameraMap.transform.rotation = Quaternion.Euler(90, avatar.transform.eulerAngles.y, 0);
                avatar.GetComponent<Car>().allowUse = true;
            }
        }

        if (_joined && Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenu();
        }

        if (_joined && Input.GetKeyDown(KeyCode.R))
        {
            radioPanel.SetActive(!radioPanel.activeSelf);
        }
    }

    private void MoveCameraFreeMouse()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        cameraCenter.transform.rotation = Quaternion.Euler(0f, cameraCenter.transform.eulerAngles.y + mouseX, 0f);
    }


    private bool IsAlreadyJoined()
    {
        bool found = false;
        Alteruna.Avatar[] avatars = FindObjectsOfType<Alteruna.Avatar>();
        foreach (Alteruna.Avatar avatar in avatars.Where(x => x.GetComponent<AI_Car>() == null)) if (avatar.IsMe) found = true;
        return found;
    }
    public void PauseMenu()
    {
        pausePanel.SetActive(!pausePanel.activeSelf);
    }

    public void OpenSettings()
    {
        settingPanel.SetActive(!settingPanel.activeSelf);
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
