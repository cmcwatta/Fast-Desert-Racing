using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alteruna;
using System;

public class Car : AttributesSync
{
    [Header("Wheels")]
    [SerializeField]
    private Wheel[] wheelsCollider;
    [SerializeField]
    private float steeringRange;
    [SerializeField]
    private float steeringSpeed;

    [Header("Properties")]
    [SerializeField]
    private float speed;
    private float _curSpeed;
    [SerializeField]
    private float brake;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private float centreOfGravityOffset = -1f;
    [SerializeField]
    public bool allowUse;

    [Header("Mod")]
    [SerializeField]
    public Mesh[] bodies;
    private int _curBody;
    [SerializeField]
    public MeshFilter meshFilter;

    [Header("Multiplayer")]
    private Alteruna.Avatar _avatar;

    void Awake()
    {
        rb.centerOfMass += Vector3.up * centreOfGravityOffset;

        _avatar = GetComponent<Alteruna.Avatar>();
    }

    void Update()
    {
        if (!allowUse) return;
        if (!_avatar.IsMe) return;

        float vInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");

        UpdateMovement(vInput, hInput);

        BroadcastRemoteMethod("ChangeStyle", _avatar.name, PlayerPrefs.GetInt("Variation"));
    }

    [SynchronizableMethod]
    public void ChangeStyle(string name, int variable)
    {
        Car car = GameObject.Find(name)?.GetComponent<Car>();
        if (car)
        {
            try
            {
                car.meshFilter.mesh = car.bodies[variable];
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

    void UpdateMovement(float vInput, float hInput)
    {
        foreach (var wheel in wheelsCollider)
        {
            if (wheel.steerable)
            {
                wheel.WheelCollider.steerAngle = hInput * steeringRange;
            }

            if (vInput != 0)
            {
                if (wheel.motorized)
                {
                    wheel.WheelCollider.motorTorque = vInput * speed * 100;
                }
                wheel.WheelCollider.brakeTorque = 0;
            }

            if (Input.GetKey(KeyCode.Space))
            {
                wheel.WheelCollider.brakeTorque = brake * 50;

                wheel.WheelCollider.motorTorque = 0;
            }
        }

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, Mathf.Clamp(transform.rotation.eulerAngles.z, -0.5f, 0.5f));
    }

    public int GetVariation()
    {
        return _curBody;
    }

    public void SwitchBodies(bool right)
    {
        _curBody += right ? 1 : -1;
        if (_curBody >= bodies.Length) _curBody = 0;
        else if (_curBody < 0) _curBody = bodies.Length - 1;
        meshFilter.mesh = bodies[_curBody];
    }

}
