using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField] private float health = 10;


public void HitDamage(float damage)
{
    health -= damage;

    if (health <= 0)
        Destroy(gameObject);
}

public float GetHealth() { return health; }
}
