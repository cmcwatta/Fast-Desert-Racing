using Alteruna;
using Alteruna.SyncedEvent;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class RacingManager : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera vCam;

    private Multiplayer _multiplayer;
    private bool _joined = false;
    [SerializeField]
    private GameObject connectingPanel;

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
    }


    private void JoinedRoom(Multiplayer multiplayer, Room room, User user)
    {
        Spawner spawner = GameObject.Find("Multiplayer").GetComponent<Spawner>();
        Car car = spawner.Spawn(PlayerPrefs.GetString("Model"), transform.position).GetComponent<Car>();
        car.GetComponent<Alteruna.Avatar>().Possessed(user);
        _joined = true;
    }

    private void Update()
    {
        connectingPanel.SetActive(!_joined);

        Alteruna.Avatar[] avatars = FindObjectsOfType<Alteruna.Avatar>();

        foreach (Alteruna.Avatar avatar in avatars)
        {
            if (avatar.IsMe)
            {
                vCam.LookAt = avatar.transform;
                vCam.Follow = avatar.transform;
                avatar.GetComponent<Car>().allowUse = true;
            }
        }
    }
}
