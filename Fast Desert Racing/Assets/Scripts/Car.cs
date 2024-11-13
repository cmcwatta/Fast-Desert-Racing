using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    [Header("Wheels")]
    [SerializeField]
    private Transform[] wheels;
    [SerializeField]
    private Transform[] wheelsSteering;
    [SerializeField]
    private float steeringRange;
    [SerializeField]
    private float steeringSpeed;

    [Header("Properties")]
    [SerializeField]
    private float speed;
    private float _curSpeed;
    [SerializeField]
    private Rigidbody rb;

    void Start()
    {
        
    }

    void Update()
    {
        float vInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");

        UpdateMovement(vInput, hInput);
        UpdateWheels(vInput, hInput);
    }

    void UpdateWheels(float vInput, float hInput)
    {
        foreach (Transform wheel in wheels)
        {
            wheel.Rotate(vInput > 0 ? rb.velocity.magnitude : -rb.velocity.magnitude, 0, 0);
        }

        foreach (Transform wheel in wheelsSteering)
        {
            wheel.localRotation = Quaternion.Lerp(
            wheel.localRotation,
            Quaternion.Euler(0, hInput * steeringRange, 0),
            Time.deltaTime * steeringSpeed);
        }
    }

    void UpdateMovement(float vInput, float hInput)
    {
        rb.AddForce(transform.forward * vInput * speed, ForceMode.Acceleration);

        transform.Rotate(0, ((hInput * (steeringSpeed * Time.deltaTime)) * rb.velocity.magnitude), 0);
    }
}
