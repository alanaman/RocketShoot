using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseAbility : MonoBehaviour
{
    [SerializeField] private GameObject pulsePrefab;

    void Start()
    {
        GameInput.I.OnAbility1Used += SpawnPulse;
    }
    private void SpawnPulse()
    {
        GameObject pulse = Instantiate(pulsePrefab, transform.position, Quaternion.identity);
        pulse.GetComponent<Pulse>().spawningBody = GetComponent<Rigidbody>();
    }
}
