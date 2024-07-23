using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    [SerializeField] public Checkpoint[] checkpoints;

    public static FinishLine I { get; private set; }

    int position = 1;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(this);
            return;
        }

        I = this;
    }

    public void OnFinishLineEnter(Racer racer)
    {
        if (racer.checkpoints.Count > 0)
        {
            if(racer.gameObject == GameManager.I.Player)
            {
                Debug.Log("You finished in position " + position + " with a time of " + GameManager.I.timer + " seconds!");
                Loader.Load(Loader.Scene.MainMenu);
            }
            racer.finishedPosition = position;
            position++;
        }
    }

}
