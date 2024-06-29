using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunShooter : MonoBehaviour
{
    [SerializeField] Transform targetingBone;
    [SerializeField] float gunRange = 50;
    [SerializeField] float reloadTime = 2;

    [SerializeField] GameObject bullet;
    [SerializeField] Transform[] bulletSpawn;
    [SerializeField] float bulletSpeed = 100;
    [SerializeField] float inaccuracy = 0.1f;
    [SerializeField] float fov = 45;

    int bulletSpawnIdx = 0;
    private float reloadTimer;


    void Start()
    {
        reloadTimer = reloadTime;
    }

    void Update()
    {
        int layer = 1 << LayerMask.NameToLayer("Enemies");
        Collider[] nearbyEnemies = Physics.OverlapSphere(
            transform.position, 
            gunRange, 
            layer, 
            QueryTriggerInteraction.Ignore);

        Enemy nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach(Collider enemyCollider in nearbyEnemies)
        {
            if(enemyCollider.gameObject.TryGetComponent(out Enemy enemy))
            {
                //check if enemy is in fov of gun 
                if(Vector3.Angle(transform.forward, enemy.transform.position - transform.position) > fov)
                    continue;
                nearestEnemy = enemy;
                nearestDistance = Vector3.Distance(transform.position, enemy.transform.position);
            }
        }

        if (nearestEnemy != null)
        {
            Vector3 enemyPos = nearestEnemy.transform.position;
            // predict position
            enemyPos += 
                nearestEnemy.GetComponent<Rigidbody>().velocity *
                Vector3.Distance(transform.position, enemyPos) /
                bulletSpeed;


            //TODO: fix out of plane 
            targetingBone.LookAt(enemyPos, Vector3.up);
            //targetingBone.Rotate(-90, 180, 0);

            reloadTimer -= Time.deltaTime;
            if (reloadTimer < 0)
            {
                reloadTimer = reloadTime;
                Fire();
            }
        }
        else
        {
            targetingBone.localRotation = Quaternion.identity;
        }
    }

    void Fire()
    {
        

        Vector3 direction = targetingBone.forward;
        Bullet bullet_inst = Instantiate(bullet, bulletSpawn[bulletSpawnIdx].position, Quaternion.LookRotation(direction, Vector3.up)).GetComponent<Bullet>();
        Vector2 rand = Random.insideUnitCircle * inaccuracy;

        direction = (direction + new Vector3(rand.x, 0, rand.y)).normalized;
        bullet_inst.Init(direction * bulletSpeed);

        bulletSpawnIdx = (bulletSpawnIdx + 1) % bulletSpawn.Length;
    }
}
