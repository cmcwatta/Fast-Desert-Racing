using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class RacingManager : MonoBehaviour
{
    [SerializeField]
    private GameObject originalCar;
    [SerializeField]
    private CinemachineVirtualCamera vCam;
    private GameObject _car;


    void Start()
    {
        //if (GameData.CarPrefab)
        //{
        //    _car = Instantiate(GameData.CarPrefab, originalCar.transform.position, originalCar.transform.rotation);
        //    Destroy(originalCar);
        //    Destroy(GameData.CarPrefab);
        //}
        //else _car = originalCar;

        //vCam.LookAt = _car.transform;
        //vCam.Follow = _car.transform;
        //_car.GetComponent<Car>().allowUse = true;
    }

    private void Update()
    {
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
