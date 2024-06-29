using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavObstacle : MonoBehaviour
{
    void Update()
    {
        
    }

    private void OnEnable()
    {
        //check if enabled
        if (gameObject.activeInHierarchy)
            GameManager.I.navTrack.UpdateObstacle(GetComponent<Collider>());
    }

    private void OnDisable()
    {
        GameManager.I.navTrack.UpdateObstacle(GetComponent<Collider>());
    }

    private void OnDestroy()
    {
        //check if enabled
        if(gameObject.activeInHierarchy)
            GameManager.I.navTrack.UpdateObstacle(GetComponent<Collider>());
    }
}
