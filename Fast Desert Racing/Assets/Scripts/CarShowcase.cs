using System;
using System.Collections;
using System.Collections.Generic;
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

    // Start is called before the first frame update
    void Start()
    {
        SpawnCar();
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
                showcasePlatform.Rotate(Vector3.down * hInput, rotateSpeed);
            }
            else showcasePlatform.Rotate(Vector3.down * lastInput, rotateSpeed / 10);


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
        GameData.CarPrefab.transform.parent = null;

        DontDestroyOnLoad(GameData.CarPrefab);

        SceneManager.LoadScene("Racing");
    }
}
