using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierPickup : TriggerParent
{
    [SerializeField] private List<Barrier> barriers;
    public override void Trigger()
    {
        foreach (Barrier barrier in barriers)
            barrier.Disable();
    }
}

