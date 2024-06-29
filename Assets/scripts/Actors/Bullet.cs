using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Bullet : MonoBehaviour
{
    [SerializeField] VisualEffect bulletVfx;
    [SerializeField] GameObject bulletViz;
    [SerializeField] float damage = 0.1f;
    float bulletVfxLifetime = 1;

    public void Init(Vector3 initialVelocity)
    {
        GetComponent<Rigidbody>().velocity = initialVelocity;
    }

    void OnTriggerEnter(Collider other)
    {
        bulletVfx.SendEvent("OnExplode");
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        Destroy(bulletViz);
        Destroy(gameObject, bulletVfxLifetime);
        if (other == GameManager.I.PlayerCollider)
        {
            GameManager.I.Player.HitDamage(damage);
        }
        else if(other.TryGetComponent<Enemy>(out Enemy enemy))
        {
            enemy.HitDamage(damage);
        }
    }

}
