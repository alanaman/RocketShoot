using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.AI;

using UnityEngine;
using static Unity.Burst.Intrinsics.X86;


// This should be on NavMeshAgent which is a child of the RigidBody
public class NavMeshAgentUtil : MonoBehaviour
{
    public void CalculatePath(Vector3 targetPos, NavMeshPath path)
    {
        NavMeshAgent nma = GetComponent<NavMeshAgent>();
        nma.CalculatePath(targetPos, path);
    }

    void Update()
    {
        //if rigibody is inside surface then reposition this agent to correct drifting
        if (NavMesh.SamplePosition(transform.parent.position, out _, 10, NavMesh.AllAreas))
        {
            transform.localPosition = Vector3.zero;
        }
    }
}
