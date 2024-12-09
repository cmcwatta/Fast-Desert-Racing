using Alteruna;
using Alteruna.SyncedEvent;
using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TMPro;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RacingManager : AttributesSync
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
    private Slider nitroSlider;
    public static Action<float> OnNitroUpdate;
    [SerializeField]
    private GameObject missilesUIObject;
    public static Action<int> OnMissilesCountUpdate;

    [SerializeField]
    private GameObject playerListUI;
    [SerializeField]
    private GameObject playerListPopulate;
    [SerializeField]
    private GameObject playerInfoTemplate;
    private Dictionary<string, int> _playerScoreInfo = new Dictionary<string, int>();
    public static Action<string, int> OnScorePlayerAdd;

    [SerializeField]
    private Transform[] spawnpoints;

    [SerializeField]
    private int nukeScore;
    [SerializeField]
    private NukeHandle nk;
    private bool nuked;

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
            speedometerText.text = Math.Round(speed * 2).ToString("0") + " KM/H";
        };
        OnNitroUpdate += delegate (float percentage)
        {
            nitroSlider.value = percentage;
        };
        OnMissilesCountUpdate += delegate (int count)
        {
            int index = 0;
            try
            {
                foreach (Image image in missilesUIObject.GetComponentsInChildren<Image>())
                {
                    image.enabled = index < count;
                    index++;
                }
            }
            catch (Exception _) { }
        };
        OnScorePlayerAdd += AddScoreToPlayers;
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
        gamePanel.SetActive(_joined && !nuked);

        Alteruna.Avatar[] avatars = FindObjectsOfType<Alteruna.Avatar>();

        foreach (Alteruna.Avatar avatar in avatars.Where(x => x.GetComponent<Car>() != null))
        {
            if (avatar.IsMe)
            {
                MoveCameraFreeMouse();
                if (Input.GetMouseButton(1))
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

        if (_joined && Input.GetKeyDown(KeyCode.P))
        {
            OpenSettings();
        }

        if (_joined && Input.GetKey(KeyCode.Tab))
        {
            playerListUI.SetActive(true);
        }
        else
        {
            playerListUI.SetActive(false);
        }

        if (_joined && Input.GetKeyDown(KeyCode.Tab))
        {
            UpdatePlayerList();
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

    private void AddScoreToPlayers(string name, int score)
    {
        if (_multiplayer.Me.IsHost)
        {
            if (_playerScoreInfo.ContainsKey(name)) _playerScoreInfo[name] += score;
            else _playerScoreInfo[name] = score;

            BroadcastRemoteMethod("UpdateScoreRPC", _playerScoreInfo);

            if (_playerScoreInfo[name] >= nukeScore)
            {
                BroadcastRemoteMethod("NukeRPC");
            }

            Debug.Log("Player Add Score");
        }
    }

    [SynchronizableMethod]
    public void NukeRPC()
    {
        nk.StartNuke();
        nuked = true;
    }


    [SynchronizableMethod]
    public void UpdateScoreRPC(Dictionary<string, int> scoreInfo)
    {
        _playerScoreInfo = scoreInfo;
    }

    private void UpdatePlayerList()
    {
        foreach (var pInfo in playerListPopulate.GetComponentsInChildren<RectTransform>())
        {
            if (pInfo.CompareTag("PlayerInfo") && pInfo.gameObject.activeSelf)
            {
                Destroy(pInfo.gameObject);
            }
        }

        Alteruna.Avatar[] avatars = FindObjectsOfType<Alteruna.Avatar>().Where(x => x.GetComponent<Car>() != null).ToArray();
        foreach (var avatar in avatars)
        {
            GameObject template = Instantiate(playerInfoTemplate, playerListPopulate.transform);
            template.SetActive(true);
            if (avatar.IsMe) template.GetComponent<Image>().color = new UnityEngine.Color(44f / 255f, 231f / 255f, 255f / 255f);
            TMP_Text[] texts = template.GetComponentsInChildren<TMP_Text>();
            texts[0].text = avatar.Owner.Name;
            if (_playerScoreInfo.ContainsKey(avatar.Owner.Name))
            {
                texts[1].text = _playerScoreInfo[avatar.Owner.Name].ToString();
            }
        }
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
    public void RestartLevel()
    {
        SceneManager.LoadScene(1);
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
