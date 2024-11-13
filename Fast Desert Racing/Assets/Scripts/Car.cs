using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
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
    private bool allowUse;

    [Header("Mod")]
    [SerializeField]
    private Mesh[] bodies;
    private int _curBody;
    [SerializeField]
    private MeshFilter meshFilter;

    void Start()
    {
        rb.centerOfMass += Vector3.up * centreOfGravityOffset;
    }

    void Update()
    {
        if (!allowUse) return;

        float vInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");

        UpdateMovement(vInput, hInput);
        UpdateWheels(vInput, hInput);
    }

    void UpdateWheels(float vInput, float hInput)
    {
    }

    void UpdateMovement(float vInput, float hInput)
    {
        //rb.AddForce(transform.forward * vInput * speed, ForceMode.Acceleration);

        //transform.Rotate(0, ((hInput * (steeringSpeed * Time.deltaTime)) * rb.velocity.magnitude), 0);

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
    }

    public void SwitchBodies(bool right)
    {
        _curBody += right ? 1 : -1;
        if (_curBody >= bodies.Length) _curBody = 0;
        else if (_curBody < 0) _curBody = bodies.Length - 1;
        meshFilter.mesh = bodies[_curBody];
    }

}
