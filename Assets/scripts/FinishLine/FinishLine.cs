using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    [SerializeField] Transform checkpoints;

/*    public void OnActivate()
    {
        finishTrigger.enabled = true;
    }

    public void OnDeactivate()
    {
        finishTrigger.enabled = false;
    }*/

    public void OnFinishLineEnter()
    {
        if (checkpoints.childCount > 0)
            return;

        Debug.Log(GameManager.I.timer);
        Loader.Load(Loader.Scene.MainMenu);
    }

}
