using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class EnemyPilot : MonoBehaviour
{
    [SerializeField] private float accelerationMax = 30;

    [SerializeField] private float inertia = 125;
    [SerializeField] private float minDistance = 20;
    [SerializeField] private float maxDistance = 30;
    [SerializeField] private float speed = 5;

    [SerializeField] private float maxTurnSpeed = 1;


    GameObject targetToDestory;

    PidController pidController;

    private Vector3 _targetDir;

    private void Awake()
    {
        var rb = GetComponent<Rigidbody>();
        rb.inertiaTensor = new Vector3(inertia, inertia, inertia);
        pidController = new PidController(transform);
        _targetDir = GetCurrentDirection();
    }

    private void Start()
    {
        targetToDestory = GameManager.I.Player.gameObject;
    }
    private void FixedUpdate()
    {
        if(targetToDestory == null)
            return;

        Vector3 targetPos = targetToDestory.transform.position;

        NavMeshPathUtil nma = GetComponentInChildren<NavMeshPathUtil>();

        NavMeshPath path = new NavMeshPath();
        nma.CalculatePath(targetPos, path);
        if(path.status != NavMeshPathStatus.PathComplete)
        {
            targetPos = GameManager.I.navTrack.GetNextTargetPosition(transform, targetToDestory.transform);
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
            float distance = Vector3.Distance(targetPos, transform.position);

            if (distance < minDistance)
            {
                Vector3 targetDirection = -(targetPos - transform.position).normalized;
                TurnAndMove(targetDirection * (minDistance - distance));
            }
            else if (distance > maxDistance)
            {
                Vector3 targetDirection = (targetPos - transform.position).normalized;
                TurnAndMove(targetDirection * (distance - maxDistance));
            }
            else
            {
                TurnAndMove(Vector3.zero);
            }
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
        return transform.rotation * new Vector3(0, 0, -1);
    }
}
