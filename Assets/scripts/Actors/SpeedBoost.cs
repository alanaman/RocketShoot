using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    HashSet<Rigidbody> _affectedRigidbodies = new HashSet<Rigidbody>();
    [SerializeField] private float _boostAccelaration = 50;

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.gameObject.GetComponentInParent<Rigidbody>();
        if(rb != null)
        {
            _affectedRigidbodies.Add(rb);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.gameObject.GetComponentInParent<Rigidbody>();
        if (rb != null)
        {
            _affectedRigidbodies.Remove(rb);
        }
    }

    private void FixedUpdate()
    {
        _affectedRigidbodies.RemoveWhere(rb => rb == null);
        foreach (Rigidbody rb in _affectedRigidbodies)
        {
            rb.AddForce(transform.forward * _boostAccelaration, ForceMode.Acceleration);
        }
    }
}
