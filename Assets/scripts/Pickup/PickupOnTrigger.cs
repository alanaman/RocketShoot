using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupOnTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GetComponentInParent<Pickup>().OnChildTriggerEnter(other);
    }
    private void OnTriggerExit(Collider other)
    {
        GetComponentInParent<Pickup>().OnChildTriggerExit(other);
    }
}
