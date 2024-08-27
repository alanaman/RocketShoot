using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Racer : MonoBehaviour
{
    public HashSet<Checkpoint> checkpoints { get; private set; }

    public Transform currentTarget { get; private set; }

    [HideInInspector]
    public int finishedPosition = -1;

    void Start()
    {
        checkpoints = new HashSet<Checkpoint>(FinishLine.I.checkpoints);
        OnCheckpointReached(null);
    }

    public void OnCheckpointReached(Checkpoint checkpoint)
    {
        checkpoints.Remove(checkpoint);
        currentTarget = null;
        float minDist = Mathf.Infinity;
        foreach (Checkpoint newTarget in checkpoints)
        {
            float dist = GameManager.I.navTrack.GetLossyDistance(transform.position, newTarget.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                currentTarget = newTarget.transform;
            }
        }
        if (currentTarget == null)
        {
            currentTarget = FinishLine.I.transform;
        }
    }

    public bool HasFinished()
    {
        return finishedPosition > 0;
    }


}
