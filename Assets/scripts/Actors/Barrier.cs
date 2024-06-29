using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour
{
    [SerializeField] GameObject barrierViz;
    [SerializeField] Collider barrier;

    public void Disable()
    {
        Destroy(gameObject);
    }
}
