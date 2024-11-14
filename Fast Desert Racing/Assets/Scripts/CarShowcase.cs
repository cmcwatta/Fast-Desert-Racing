using Alteruna;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class GameData
{
    public static GameObject CarPrefab;
}

public class CarShowcase : MonoBehaviour
{
    [Header("Platform")]
    [SerializeField]
    private Transform showcasePlatform;
    [SerializeField]
    private Transform spawnLocation;
    [SerializeField]
    private float rotateSpeed;
    [SerializeField]
    private bool allowRotate;
    private float lastInput = 1;
    private bool switchedStyle = false;
    [SerializeField]
    private TMP_Text carName;

    [Header("Cars")]
    [SerializeField]
    private GameObject[] cars;
    private int _curCarIndex = 0;

    private Multiplayer _multiplayer;

    // Start is called before the first frame update
    void Start()
    {
        SpawnCar();

        _multiplayer = FindObjectOfType<Multiplayer>();
        _multiplayer.OnConnected.AddListener((Multiplayer a, Endpoint _) => a.RefreshRoomList());
        _multiplayer.OnRoomListUpdated.AddListener((Multiplayer call) =>
        {
            if (!call.AvailableRooms.Any(r => r.Name == "MainServer") || call.AvailableRooms.Count <= 0) _multiplayer.CreateRoom("MainServer", false, 0, false, false);
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (allowRotate)
        {
            float hInput = Input.GetAxis("Horizontal");
            float vInput = Input.GetAxis("Vertical");
            if (hInput != 0)
            {
                lastInput = hInput;
                showcasePlatform.Rotate(Vector3.down * hInput, Time.deltaTime * rotateSpeed);
            }
            else showcasePlatform.Rotate(Vector3.down * lastInput, Time.deltaTime * rotateSpeed / 10);


            if (vInput != 0 && !switchedStyle)
            {
                GameData.CarPrefab?.GetComponent<Car>().SwitchBodies(vInput > 0 ? true : false);
                switchedStyle = true;
            }
            if (vInput == 0) switchedStyle = false;
        }
    }

    public void Switch(bool right)
    {
        SpawnCar(right, true);
    }

    void SpawnCar(bool right = false, bool notSwitch = false)
    {
        if (GameData.CarPrefab) Destroy(GameData.CarPrefab);
        if (notSwitch) _curCarIndex += right ? 1 : -1;
        if (_curCarIndex >= cars.Length) _curCarIndex = 0;
        else if (_curCarIndex < 0) _curCarIndex = cars.Length - 1;
        GameObject car = cars[notSwitch ? _curCarIndex : 0];

        GameData.CarPrefab = Instantiate(car, spawnLocation.position, Quaternion.identity, showcasePlatform);

        carName.text = car.name;
    }

    public void StartPlaying()
    {
        PlayerPrefs.SetString("Model", cars[_curCarIndex].name);
        PlayerPrefs.SetInt("Variation", GameData.CarPrefab?.GetComponent<Car>().GetVariation() ?? 0);

        SceneManager.LoadScene("Racing");
    }
}
