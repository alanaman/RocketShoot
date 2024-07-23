using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class RivalPilot : MonoBehaviour
{
    [SerializeField] private float accelerationMax = 30;

    [SerializeField] private float inertia = 125;
    //[SerializeField] private float minDistance = 20;
    //[SerializeField] private float maxDistance = 30;
    [SerializeField] private float speed = 5;

    [SerializeField] private float maxTurnSpeed = 1;



    Racer racer;
    NavMeshPath path;

    PidController pidController;

    private Vector3 _targetDir;
    NavMeshAgentUtil nma;

    private void Awake()
    {

        _targetDir = GetCurrentDirection();
    }

    private void Start()
    {
        var rb = GetComponent<Rigidbody>();
        rb.inertiaTensor = new Vector3(0, inertia, 0);
        pidController = new PidController(transform);


        nma = GetComponentInChildren<NavMeshAgentUtil>();
        racer = GetComponent<Racer>();
        path = new NavMeshPath();
    }
    private void Update()
    {


        Vector3 targetPos = racer.currentTarget.position;

        

        nma.CalculatePath(targetPos, path);
        if(path.status != NavMeshPathStatus.PathComplete)
        {
            targetPos = GameManager.I.navTrack.GetNextTargetPosition(transform, racer.currentTarget);
            nma.CalculatePath(targetPos, path);
            Vector3 target = path.corners[1];
            TurnAndMove(target - transform.position);
            return;
        }
        if(path.status != NavMeshPathStatus.PathComplete)
        {
            //no way to get to target
            return;
        }

        Vector3[] corners = path.corners;

        if(corners.Length <= 2)
        {
            TurnAndMove(targetPos - transform.position);
        }
        else
        {
            Vector3 target = corners[1];

            TurnAndMove(target - transform.position);
        }
    }

    public NavMeshPath GetNavMeshPath()
    {
        return path;
    }

    private void TurnAndMove(Vector3 targetVector)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 velocity = rb.velocity;

        Vector3 dirCurrent = GetCurrentDirection().normalized;

        float timeToDestination = targetVector.magnitude / speed + Mathf.Epsilon;

        Vector3 acc = 2 * (targetVector - velocity * timeToDestination) / timeToDestination * timeToDestination;

        _targetDir = Vector3.RotateTowards(_targetDir, acc.normalized, maxTurnSpeed * Time.deltaTime, 0);

        pidController.ApplyTorque(dirCurrent, _targetDir);

        if (Vector3.Dot(acc, dirCurrent) < 0)
            return;

        Vector3 force = Vector3.Dot(acc, dirCurrent) * dirCurrent;
        if (force.magnitude > accelerationMax)
            force = force.normalized * accelerationMax;

        GetComponent<Rigidbody>().AddForce(force);

    }

    private Vector3 GetCurrentDirection()
    {
        return transform.rotation * new Vector3(0, 0, 1);
    }
}
