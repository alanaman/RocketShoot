using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Rocket : MonoBehaviour
{
    enum RocketState
    {
        Idle,
        Fired,
        Exploded
    }

    RocketState rocketState = RocketState.Idle;
    GameObject target;

    [SerializeField] float speed = 15.0f;
    [SerializeField] float angularSpeed = 100.0f;
    [SerializeField] int damage = 1;


    [SerializeField] private GameObject rocketViz;
    private float rocketDestoyDelay = 0.1f;

    RotationController rotController;

    [SerializeField] VisualEffect rocketVfx;


    void Start()
    {
        rotController = new RotationController(transform);
        rotController.angularSpeed = angularSpeed;
        target = GameManager.I.Player.gameObject;
    }

    void FixedUpdate()
    {
        if(rocketState == RocketState.Fired)
        {
            if(target != null)
                rotController.ApplyRotation(transform.up, (target.transform.position - transform.position).normalized);
            transform.position += transform.up * speed * Time.deltaTime;
        }
    }

    public void SetTarget(GameObject target)
    {
        this.target = target;        
    }

    public void Fire()
    {
        if(rocketState == RocketState.Idle)
        {
            rocketState = RocketState.Fired;
            rocketVfx.SendEvent("OnFire");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GetComponent<Collider>().enabled = false;
        rocketVfx.SendEvent("OnExplode");
        rocketState = RocketState.Exploded;
        Destroy(rocketViz, rocketDestoyDelay);
        Destroy(gameObject, rocketVfx.GetFloat("maxLifetime"));
        if(other == GameManager.I.PlayerCollider) 
        {
            GameManager.I.Player.HitDamage(damage);
        }
    }

}
