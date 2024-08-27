using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pulse : MonoBehaviour
{
    Collider sphereCollider;

    public float initialRadius = 0.1f;
    public float pulseDuration = .2f;
    public float finalRadius = 5f;
    public float impulseVelocity = 100f;

    float timeSinceSpawn = 0;

    public Rigidbody spawningBody;
    Vector3 velocity;
    
    [SerializeField] Renderer pulseRenderer;


    void Start()
    {
        sphereCollider = GetComponent<Collider>();
        gameObject.transform.localScale = new Vector3(initialRadius, initialRadius, initialRadius);
        if(spawningBody != null)
        {
            velocity = spawningBody.velocity;
        }
        else {
            velocity = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        timeSinceSpawn += Time.deltaTime;
        float scale = Mathf.Lerp(initialRadius, finalRadius, timeSinceSpawn / pulseDuration);
        transform.localScale = new Vector3(scale, scale, scale);
        transform.position += velocity * Time.deltaTime;
        pulseRenderer.material.SetFloat("_alpha", 1 - timeSinceSpawn / pulseDuration);

        if (timeSinceSpawn > pulseDuration)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.gameObject.GetComponentInParent<Rigidbody>();
        if (rb != null && rb != spawningBody)
        {
            Vector3 centerToObject = (other.transform.position - transform.position);
            float scaledImpulseVelocity = impulseVelocity * Mathf.Lerp(1,0, (centerToObject.magnitude-initialRadius)/finalRadius);
            rb.AddForce(centerToObject.normalized * scaledImpulseVelocity, ForceMode.VelocityChange);
        }

    }
}
