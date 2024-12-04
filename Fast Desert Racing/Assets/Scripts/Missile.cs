using Alteruna;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using Avatar = Alteruna.Avatar;

public class Missile : MonoBehaviour
{
    [SerializeField]
    private float speed;
    [SerializeField]
    private float destructAfter;
    [SerializeField]
    private GameObject missileBody;
    [SerializeField]
    private GameObject explosionObject;
    [SerializeField]
    private float damage;

    private Rigidbody _rb;
    private Avatar _avatar;

    private bool destroyed;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();    
        _avatar = _rb.GetComponent<Avatar>();

        StartCoroutine(DestroyAfter());
    }

    void Update()
    {
        if (_avatar.IsMe)
        {
            if (destroyed) return;
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    private IEnumerator DestroyAfter()
    {
        yield return new WaitForSeconds(destructAfter);
        DestroyMissile();
    }

    private void DestroyMissile()
    {
        Spawner spawner = GameObject.Find("Multiplayer").GetComponent<Spawner>();
        spawner.Despawn(this.gameObject);
    }

    Transform GetHighestParent(Transform obj)
    {
        while (obj.parent != null)
        {
            obj = obj.parent;
        }
        return obj;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (_avatar.IsMe)
        {
            Transform hit = other.transform;

            Avatar hitAvatar = GetHighestParent(hit).GetComponent<Avatar>();
            if (hitAvatar != null)
            {
                if (hitAvatar.IsMe)
                {
                    return;
                }
                else
                {
                    Car car = hitAvatar.GetComponent<Car>();
                    if (car != null)
                    {
                        if (car.DamageCar(hitAvatar.name, damage))
                        {
                            RacingManager.OnScorePlayerAdd?.Invoke(_avatar.Owner.Name, 1);
                        }
                    }
                }
            }

            destroyed = true;
            missileBody.transform.position = new Vector3(6969, 6969, 6969);
            Spawner spawner = GameObject.Find("Multiplayer").GetComponent<Spawner>();
            ExplosionDelay explosion = spawner.Spawn(explosionObject.name, transform.position).GetComponent<ExplosionDelay>();
            if (explosion != null)
            {
                explosion.OnDestroyExplosion += delegate ()
                {
                    DestroyMissile();
                };
            }
        }
    }
}
