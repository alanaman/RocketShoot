using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.AI;

using UnityEngine;
using static Unity.Burst.Intrinsics.X86;

public class NavMeshPathUtil : MonoBehaviour
{
    // Start is called before the first frame update
    public void CalculatePath(Vector3 targetPos, NavMeshPath path)
    {
        NavMeshAgent nma = GetComponent<NavMeshAgent>();
        nma.CalculatePath(targetPos, path);
    }
}
