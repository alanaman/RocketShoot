using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other == GameManager.I.PlayerCollider)
        {
            GetComponentInParent<FinishLine>().OnFinishLineEnter();
        }
    }
}
