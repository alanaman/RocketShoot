using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] float triggerTime = 0.1f;

    private float pickupProgress = -0.01f;
    private bool hasEntered = false;

    [SerializeField] Renderer progressRenderer;

    void Update()
    {
        if (hasEntered)
        {
            pickupProgress += Time.deltaTime;
            if (pickupProgress > triggerTime)
            {
                Destroy(gameObject);
            }
        }
        else if (pickupProgress > -0.01f)
        {
            pickupProgress -= Time.deltaTime;
        }
        progressRenderer.material.SetFloat("_progress", pickupProgress / triggerTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == GameManager.I.PlayerCollider)
        {
            hasEntered = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other == GameManager.I.PlayerCollider)
        {
            hasEntered = false;
        }
    }
}
