using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alteruna;
using System;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;
using Input = UnityEngine.Input;
using Unity.Properties;

public class Car : AttributesSync
{
    [Header("Wheels")]
    [SerializeField]
    public Wheel[] wheelsCollider;
    [SerializeField]
    private float steeringRange;
    [SerializeField]
    private float steeringSpeed;

    [Header("Properties")]
    [SerializeField]
    private float speed;
    [SerializeField]
    private float turboSpeed;
    [SerializeField]
    private GameObject turboObject;
    private bool _turboEnabled;
    private float _turboUsage;

    public float CurSpeed;
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

    [SerializeField]
    public AudioSource hornAudio;

    [SerializeField]
    private GameObject particleParent;

    [SerializeField]
    private AudioSource bumpSound;

    [SerializeField]
    private SpriteRenderer arrowMap;

    [SerializeField]
    private Sprite arrowMapMe;
    private Quaternion _originalArrRot;

    [Header("Multiplayer")]
    private Alteruna.Avatar _avatar;

    [Header("Health")]
    [SerializeField]
    private float maxHealth;
    [SerializeField]
    private float _curHealth;
    [SerializeField]
    private GameObject smokeObject;
    [SerializeField]
    private GameObject fireObject;

    [Header("Front Lights")]
    [SerializeField]
    private GameObject frontLights;
    private bool _flTurnedOn;


    private bool _isServer;

    void Awake()
    {
        rb.centerOfMass += Vector3.up * centreOfGravityOffset;

        _originalArrRot = arrowMap.transform.rotation;

        _avatar = GetComponent<Alteruna.Avatar>();

        _curHealth = maxHealth;

        _turboUsage = 100;
    }

    void Update()
    {
        CurSpeed = rb.velocity.magnitude;

        arrowMap.gameObject.SetActive(true);
        arrowMap.gameObject.transform.rotation = Quaternion.Euler(_originalArrRot.eulerAngles.x, transform.rotation.eulerAngles.y - 180, _originalArrRot.eulerAngles.z);

        UpdateHealth();

        UpdateFrontLight();

        if (!allowUse) return;
        if (!_avatar.IsMe) return;

        if (Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                _isServer = true;
            }
        }

        float vInput = UnityEngine.Input.GetAxis("Vertical");
        float hInput = UnityEngine.Input.GetAxis("Horizontal");

        arrowMap.sprite = arrowMapMe;

        if (UnityEngine.Input.GetKeyDown(KeyCode.H))
        {
            BroadcastRemoteMethod("Horn", _avatar.name);
        }
        
        UpdateTurbo();

        UpdateMovement(vInput, hInput);

        UpdateParticle();

        BroadcastRemoteMethod("ChangeStyle", _avatar.name, PlayerPrefs.GetInt("Variation"));

        if (_isServer) BroadcastRemoteMethod("UpdateServerRPC", _avatar.name);

        RacingManager.OnSpeedUpdate?.Invoke(rb.velocity.magnitude);
    }

    private void UpdateFrontLight()
    {
        if (_avatar.IsMe)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                _flTurnedOn = !_flTurnedOn;
            }
            BroadcastRemoteMethod("TurnFrontLight", _avatar.name, _flTurnedOn);
        }
        frontLights.SetActive(_flTurnedOn);
    }

    [SynchronizableMethod]
    public void TurnFrontLight(string name, bool turned)
    {
        Car car = GameObject.Find(name)?.GetComponent<Car>();
        if (car)
        {
            try
            {
                car._flTurnedOn = turned;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

    [SynchronizableMethod]
    public void UpdateServerRPC(string name)
    {
        Car car = GameObject.Find(name)?.GetComponent<Car>();
        if (car)
        {
            try
            {
                car.gameObject.transform.position = new Vector3(1000, 0, 1000);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

    private bool _usedTurbo;
    private void UpdateTurbo()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (_turboUsage > 0 && _usedTurbo == false)
            {
                _turboEnabled = true;
                _turboUsage = Math.Clamp(_turboUsage - 0.5f, 0, 100f);
                BroadcastRemoteMethod("Nitro", _avatar.name, true);
            }
            else
            {
                _turboEnabled = false;
                BroadcastRemoteMethod("Nitro", _avatar.name, false);
            }
        }
        else
        {
            _turboEnabled = false;
            _turboUsage = Math.Clamp(_turboUsage + 0.1f, 0, 100f);
            _usedTurbo = _turboUsage < 100;
            BroadcastRemoteMethod("Nitro", _avatar.name, false);
        }
    }

    [SynchronizableMethod]
    public void Nitro(string name, bool activate)
    {
        Car car = GameObject.Find(name)?.GetComponent<Car>();
        if (car)
        {
            try
            {
                car.turboObject.SetActive(activate);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

    [SynchronizableMethod]
    public void Horn(string name)
    {
        Car car = GameObject.Find(name)?.GetComponent<Car>();
        if (car)
        {
            try
            {
                car.hornAudio.Play();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
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
            if (!_alreadyTriggeredDead)
            {
                if (wheel.steerable)
                {
                    wheel.WheelCollider.steerAngle = hInput * steeringRange;
                }

                if (vInput != 0)
                {
                    float speedCalc = _turboEnabled ? turboSpeed : speed;
                    if (wheel.motorized)
                    {
                        wheel.WheelCollider.motorTorque = vInput * speedCalc * 100;
                    }
                    wheel.WheelCollider.brakeTorque = 0;
                }
            }

            if (UnityEngine.Input.GetKey(KeyCode.Space) || _alreadyTriggeredDead)
            {
                wheel.WheelCollider.brakeTorque = brake * 50;

                wheel.WheelCollider.motorTorque = 0;
            }
        }
    }

    private void UpdateParticle()
    {
        foreach (var particle in particleParent.GetComponentsInChildren<ParticleSystem>())
        {
            particle.startSize = Mathf.Clamp(rb.velocity.magnitude, 0, 2);
        }
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


    private void OnCollisionEnter(Collision collision)
    {
        bool bumpColide = false;

        foreach (var collider in collision.contacts)
        {
            if (collider.thisCollider.CompareTag("Bump")) bumpColide = true;
        }

        if (bumpColide)
        {
            bumpSound.Play();
            if (_avatar.IsMe)
            {
                BroadcastRemoteMethod("SetHealthCar", _avatar.name, _curHealth - CurSpeed);
            }
        }
    }



    [SynchronizableMethod]
    public void SetHealthCar(string name, float health)
    {
        Car car = GameObject.Find(name)?.GetComponent<Car>();
        if (car)
        {
            try
            {
                car._curHealth = health;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

    private bool _alreadyTriggeredDead;
    private void UpdateHealth()
    {
        if (_curHealth <= 0)
        {
            fireObject.SetActive(true);
            allowUse = false;
            if (_avatar.IsMe) UpdateMovement(0, 0);
            if (!_alreadyTriggeredDead)
            {
                StartCoroutine(StartDead());
                _alreadyTriggeredDead = true;
            }
        }
        else if(_curHealth <= (maxHealth / 2))
        {
            smokeObject.SetActive(true);
        }
        else
        {
            smokeObject.SetActive(false);
            fireObject.SetActive(false);
        }
    }

    private IEnumerator StartDead()
    {
        yield return new WaitForSeconds(5f);
        Spawner spawner = GameObject.Find("Multiplayer").GetComponent<Spawner>();
        GameObject explosion = spawner.Spawn("Explosion", transform.position);
        yield return new WaitForSeconds(3f);
        spawner.Despawn(explosion);
        if (_avatar.IsMe)
        {
            SceneManager.LoadScene(0);
        }
        yield return null;
    }
}
