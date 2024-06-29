using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] Transform gunTransform;
    [SerializeField] float gunRange = 50;
    [SerializeField] float reloadTime = 2;

    [SerializeField] GameObject bullet;
    [SerializeField] Transform bulletSpawn;
    [SerializeField] float bulletSpeed = 20;
    [SerializeField] float inaccuracy = 0.1f;

    private float reloadTimer;

    void Start()
    {
        reloadTimer = reloadTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.TryGetPlayer(out Player p))
            return;

        Vector3 playerPos = p.transform.position;

        if(Vector3.Distance(gunTransform.position, playerPos) < gunRange)
        {
            // predict player position
            playerPos += p.GetComponent<Rigidbody>().velocity * Vector3.Distance(gunTransform.position, playerPos) / bulletSpeed;


            //TODO: fix out of plane pointing
            gunTransform.LookAt(playerPos, Vector3.up);
            gunTransform.Rotate(-90, 180, 0);

            reloadTimer -= Time.deltaTime;
            if(reloadTimer < 0)
            {
                reloadTimer = reloadTime;
                Fire();
            }
            
        }
    }

    void Fire()
    {
        Vector3 direction = (bulletSpawn.position - gunTransform.position).normalized;
        Bullet bullet_inst = Instantiate(bullet, bulletSpawn.position, Quaternion.LookRotation(direction, Vector3.up)).GetComponent<Bullet>();
        Vector2 rand = Random.insideUnitCircle * inaccuracy;
        
        direction = (direction + new Vector3(rand.x, 0, rand.y)).normalized;
        bullet_inst.Init(direction*bulletSpeed);
    }
}
