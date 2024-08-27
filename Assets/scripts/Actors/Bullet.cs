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

    Vector3 velocity = Vector3.zero;

    public void Init(Vector3 initialVelocity)
    {
        this.velocity = initialVelocity;
    }

    private void FixedUpdate()
    {
        transform.position += velocity * Time.fixedDeltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        bulletVfx.SendEvent("OnExplode");
        Destroy(bulletViz);
        Destroy(gameObject, bulletVfxLifetime);
        if (other == GameManager.I.PlayerCollider)
        {
            GameManager.I.Player.HitDamage(damage);
        }
        else
        {
            other.GetComponentInParent<Enemy>()?.HitDamage(damage);
        }
    }

}
