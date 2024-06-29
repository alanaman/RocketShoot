using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class nav : MonoBehaviour
{
    [SerializeField] NavMeshSurface navMeshSurface;
    [SerializeField] NavMeshAgent navMeshAgent;
    // Start is called before the first frame update
    void Start()
    {
        NavMeshPath navMeshPath = new NavMeshPath();
        navMeshAgent.CalculatePath(Vector3.zero, navMeshPath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
