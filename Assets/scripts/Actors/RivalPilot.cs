using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class RivalPilot : MonoBehaviour
{
    //variable to emulate real world stick turn speed
    [SerializeField] private float maxTurnSpeed = 1;



    Racer racer;
    public ShipStats stats;
    NavMeshPath path;

    PidController pidController;

    private Vector3 _targetDir;
    NavMeshAgentUtil nma;
    Rigidbody rb;
    [SerializeField] LayerMask pathingLayerMask;

    private void Awake()
    {

        _targetDir = GetCurrentDirection();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.inertiaTensor = new Vector3(0, stats.inertia, 0);
        rb.mass = stats.mass;
        pidController = new PidController(transform);


        nma = GetComponentInChildren<NavMeshAgentUtil>();
        racer = GetComponent<Racer>();
        path = new NavMeshPath();
    }
    private void FixedUpdate()
    {
/*

        Vector3 targetPos = racer.currentTarget.position;

        

        nma.CalculatePath(targetPos, path);
        if(path.status != NavMeshPathStatus.PathComplete)
        {
            targetPos = GameManager.I.navTrack.GetNextTargetPosition(transform.position, racer.currentTarget.position);
            nma.CalculatePath(targetPos, path);
            Vector3 target = path.corners[1];
            TurnMoveAndStop(target - transform.position);
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
            TurnMoveAndStop(targetPos - transform.position);
        }
        else
        {
            Vector3 target = corners[1];

            TurnMoveAndStop(target - transform.position);
        }*/
        

        Vector3 velocity = rb.velocity;
        List<Vector3> path = GameManager.I.navTrack.GetPath(transform.position, racer.currentTarget.position, 3);

        for(int i = 0; i < path.Count-1; i++)
        {
            Debug.DrawRay(path[i], path[i+1] - path[i], Color.white);
        }

        if (path.Count == 1)
        {
            TurnAndMove(racer.currentTarget.position - transform.position);
        }
        else if (path.Count == 2)
        {
            Vector3 targetVector = racer.currentTarget.position - transform.position;
            Ray r = new Ray(transform.position, targetVector);
            if (Physics.Raycast(r, out _, (targetVector).magnitude, pathingLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (IsBetweenPoints(path[0], path[1]))
                {
                    TurnAndMove(path[1] - transform.position);
                }
                else
                {
                    TurnAndMove(path[0] - transform.position);
                }
            }
            else
            {
                TurnAndMove(targetVector);
            }
        }
        else if (path.Count == 3)
        {
            if (IsBetweenPoints(path[0], path[1]))
            {
                //try to move to path[2]
                Vector3 targetVector = path[2] - transform.position;
                Ray r = new Ray(transform.position, targetVector);
                if (Physics.Raycast(r, out _, (targetVector).magnitude, pathingLayerMask, QueryTriggerInteraction.Ignore))
                {
                    //path[2] is blocked, try to move to path[1]
                    TurnAndMove(path[1] - transform.position);
                }
                else
                {
                    TurnAndMove(targetVector);
                }
            }
            else
            {
                //try to move to path[1]
                Vector3 targetVector = path[1] - transform.position;
                Ray r = new Ray(transform.position, targetVector);
                if (Physics.Raycast(r, out _, (targetVector).magnitude, pathingLayerMask, QueryTriggerInteraction.Ignore))
                {
                    //path[1] is blocked, try to move to path[1]
                    TurnAndMove(path[0] - transform.position);
                }
                else
                {
                    TurnAndMove(targetVector);
                }
            }
        }
        else
        {
            Debug.LogError("Path length should positive and less than 4");
        }

    }

    private bool IsBetweenPoints(in Vector3 p1, in Vector3 p2)
    {
        Vector3 v1 = transform.position - p1;
        Vector3 v2 = p2 - p1;

        //Note: player is always nearer to p1 than p2 so below check is enough
        return (Vector3.Dot(v1, v2) > 0);
    }

    private void TurnAndMove(Vector3 targetVector)
    {
        //account for current velocity
        Vector3 nTargetDirection = targetVector.normalized;
        Vector3 nVelocity = rb.velocity.normalized;
        Vector3 perpComponent = nVelocity - nTargetDirection * Vector3.Dot(nVelocity, nTargetDirection);
        nTargetDirection = (nTargetDirection - perpComponent).normalized;

        //soft collision avoidance
        Collider[] colliders = Physics.OverlapSphere(transform.position, 10, pathingLayerMask, QueryTriggerInteraction.Ignore);


        Vector3 dirCurrent = GetCurrentDirection();

        pidController.ApplyTorque(dirCurrent, nTargetDirection);

        Debug.DrawRay(transform.position, targetVector, Color.red);
        Debug.DrawRay(transform.position, nTargetDirection*50, Color.green);
        Debug.DrawRay(transform.position, dirCurrent * 10, Color.blue);

        float accScale = Vector3.Dot(nTargetDirection, dirCurrent);
        if(accScale > 0)
        {
            Vector3 force = accScale * stats.maxForce * dirCurrent;
            //damping
            force -= stats.dampingConstant * rb.velocity;
            GetComponent<Rigidbody>().AddForce(force);
        }
    }

    private void TurnMoveAndStop(Vector3 targetVector)
    {
        Vector3 velocity = rb.velocity;

        Vector3 dirCurrent = GetCurrentDirection().normalized;

        float timeToDestination = targetVector.magnitude / rb.velocity.magnitude + Mathf.Epsilon;

        Vector3 acc = 2 * (targetVector - velocity * timeToDestination) / timeToDestination * timeToDestination;

        _targetDir = Vector3.RotateTowards(_targetDir, acc.normalized, maxTurnSpeed * Time.deltaTime, 0);

        pidController.ApplyTorque(dirCurrent, _targetDir);

        if (Vector3.Dot(acc, dirCurrent) < 0)
            return;

        Vector3 force = Vector3.Dot(acc, dirCurrent) * dirCurrent;
        if (force.magnitude > stats.maxForce)
            force = force.normalized * stats.maxForce;

        GetComponent<Rigidbody>().AddForce(force);

    }

    private Vector3 GetCurrentDirection()
    {
        return transform.rotation * new Vector3(0, 0, 1);
    }
}
