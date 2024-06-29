using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketPickup : TriggerParent
{
    [SerializeField] private List<Rocket> rockets;
    public override void Trigger()
    {
        foreach(Rocket rocket in rockets)
            rocket.Fire();
    }
}
