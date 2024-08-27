using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField] private float health = 10;
    public HealthBarUI healthBar;

    private void Start()
    {
        healthBar.SetMaxHeath(health);
    }

    public void HitDamage(float damage)
    {
        health -= damage;
        healthBar.SetHealth(health);

        if (health <= 0)
            Destroy(gameObject);
    }

    public float GetHealth() { return health; }
}
