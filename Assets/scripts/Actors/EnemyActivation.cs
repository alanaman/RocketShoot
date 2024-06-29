using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyActivation : MonoBehaviour
{
    [SerializeField] float activationTime = 0.5f;
    float currTime = 0.0f;
    bool hasEntered = false;

    private void Update()
    {
        if (hasEntered)
        {
            currTime += Time.deltaTime;
            if (currTime > activationTime)
            {
                GetComponentInParent<EnemyPilot>().enabled = true;
                GetComponentInParent<EnemyShooter>().enabled = true;
                Destroy(gameObject);
            }
        }
        else
        {
            currTime -= Time.deltaTime;
            currTime = Mathf.Max(0, currTime);
        }

        GetComponent<Renderer>().material.SetFloat("_progress", currTime / activationTime);
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
