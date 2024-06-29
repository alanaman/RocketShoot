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

    PidController pidController;

    private Vector3 _targetDir;
    NavMeshPathUtil nma;

    private void Awake()
    {
        var rb = GetComponent<Rigidbody>();
        rb.inertiaTensor = new Vector3(inertia, inertia, inertia);
        pidController = new PidController(transform);
        _targetDir = GetCurrentDirection();
    }

    private void Start()
    {
        nma = GetComponentInChildren<NavMeshPathUtil>();
        racer = GetComponent<Racer>();
    }
    private void FixedUpdate()
    {


        Vector3 targetPos = racer.currentTarget.position;

        

        NavMeshPath path = new NavMeshPath();
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
