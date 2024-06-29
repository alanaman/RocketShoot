using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] Transform innerCircleBone;
    [SerializeField] Transform outerCircleBone;
    [SerializeField] Transform pickupBone;
    [SerializeField] float triggerTime = 0.1f;

    public float rotSpeed;
    public float amplitude;

    private float pickupProgress = -0.01f;
    private bool hasEntered = false;

    [SerializeField] Renderer progressRenderer;

    void Update()
    {
        outerCircleBone.rotation = Quaternion.Euler(0, Time.time * rotSpeed, amplitude * Mathf.Cos(Time.time * rotSpeed / 30));
        innerCircleBone.rotation = Quaternion.Euler(0, Time.time * rotSpeed, amplitude * Mathf.Sin(Time.time * rotSpeed / 30));
        pickupBone.rotation = Quaternion.Euler(0, -Time.time * rotSpeed, 0);

        if(hasEntered)
        {
            pickupProgress += Time.deltaTime;
            if(pickupProgress > triggerTime)
            {
                Trigger();
                Destroy(gameObject);
            }
        }
        else if(pickupProgress>-0.01f)
        {
            pickupProgress -= Time.deltaTime;
        }
        progressRenderer.material.SetFloat("_progress", pickupProgress/triggerTime);
    }

    public void OnChildTriggerEnter(Collider other)
    {
        if(other == GameManager.I.PlayerCollider)
        {
            hasEntered = true;
        }
    }
    public void OnChildTriggerExit(Collider other)
    {
        if(other == GameManager.I.PlayerCollider)
        {
            hasEntered = false;
        }
    }

    public void Trigger()
    {
        GetComponent<TriggerParent>().Trigger();
    }
}
