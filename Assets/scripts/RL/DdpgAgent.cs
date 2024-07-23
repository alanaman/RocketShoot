using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Tensorflow.Binding;
using Tensorflow;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using static Unity.Burst.Intrinsics.X86;
using System.Text;
using Unity.Mathematics;
using System.Runtime.CompilerServices;
using Ddpg;

public class DdpgAgent : MonoBehaviour
{

    //TransformData startingTransform;


    [SerializeField] private float accelerationMax = 300;
    [SerializeField] private float velocityMax = 3000;
    [SerializeField] private float lateralDamp = 0.5f;
    [SerializeField] private float inertia = 125;


    private Vector3 dirTarget = new Vector3(0, 0, 1);

    PidController pidController;

    Racer racer;
    NavMeshPath path;
    NavMeshAgentUtil nma;

    int numRays = 8;
    Ray[] rays;
    int raycastLayer;

    DdpgState state;
    DdpgAction networkOutputAction;
    DdpgAction takenAction;
    TaskAwaiter<Routeguide.Action>? getActionAwaiter = null;
    Vector2 lastPosition;
    float totalReward = 0;
    float timer = 0;
    float nextTestTime;

    OUNoise ouNoise;
    Vector3 spawnPos;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.inertiaTensor = new Vector3(0, inertia, 0);
        pidController = new PidController(transform);

        //pidController.Init(transform);
        rays = new Ray[numRays];

        nma = GetComponentInChildren<NavMeshAgentUtil>();
        racer = GetComponent<Racer>();
        path = new NavMeshPath();

        raycastLayer = 1 << LayerMask.NameToLayer("Static_Track");
        spawnPos = transform.position;
        nextTestTime = DdpgTrainer.I.testInterval;
        ouNoise = new OUNoise(3);

        transform.rotation = Quaternion.Euler(new Vector3(0f, UnityEngine.Random.Range(0, 360), 0f));
        lastPosition = transform.position.xz();

    }

    private void ResetAgent()
    {
        rb.velocity = new Vector3(0f, 0f, 0f);
        rb.angularVelocity = new Vector3(0f, 0f, 0f);
        transform.rotation = Quaternion.Euler(new Vector3(0f, UnityEngine.Random.Range(0,360), 0f));
        transform.position = spawnPos;
        GetComponent<Racer>().finishedPosition = 0;
    }

    private void FixedUpdate()
    {
        if(Time.timeScale == 0)
        {
            return;
        }

        timer += Time.deltaTime;
        if(timer > nextTestTime)
        {
            nextTestTime += DdpgTrainer.I.testInterval;
            if(totalReward/timer < DdpgTrainer.I.resetThreshold)
            {
                Debug.Log("Resetting agent");
                ResetAgent();
            }
        }




        Vector3 targetPos = racer.currentTarget.position;
        nma.CalculatePath(targetPos, path);
        
        if (path.status != NavMeshPathStatus.PathComplete)
        {
            targetPos = GameManager.I.navTrack.GetNextTargetPosition(transform, racer.currentTarget);
            nma.CalculatePath(targetPos, path);
        }

        if(getActionAwaiter == null)
        {
            state = GetState();
            getActionAwaiter = DdpgTrainer.I.GetActionAsync(state);
        }
        else if(getActionAwaiter.Value.IsCompleted)
        {
            networkOutputAction = new DdpgAction(getActionAwaiter.Value.GetResult().Data.ToArray());
            getActionAwaiter = null;
        }

        takenAction = networkOutputAction;
        takenAction.AddNoise(ouNoise.Sample(), DdpgTrainer.I.noise);
        takenAction.Clip();


        //get input
        Vector2 dirInput2d = takenAction.GetDirectionVector();
        if (dirInput2d.magnitude > 1) { dirInput2d.Normalize(); }
        Vector3 dirInput = dirInput2d.To3d();
        float acc = takenAction.acc * accelerationMax;


        Vector3 dirCurrent = transform.rotation * new Vector3(0, 0, 1);
        dirCurrent.Normalize();

        Debug.DrawRay(transform.position, networkOutputAction.GetDirectionVector().To3d() * 50, Color.red);
        Debug.DrawRay(transform.position, dirInput * 50, Color.green);
        Debug.DrawCircle(transform.position + dirInput * takenAction.acc * 50, 1, 4, Color.green);
        Debug.DrawX(transform.position + dirInput * 50, 1, 4, Color.green);


        Debug.DrawRay(transform.position, dirCurrent * 10, Color.blue);

        Debug.DrawRay(transform.position, state.targets[0].To3d(), Color.white);
        Debug.DrawRay(transform.position + state.targets[0].To3d(), (state.targets[1] - state.targets[0]).To3d(), Color.white);
        Debug.DrawRay(transform.position + state.targets[1].To3d(), (state.targets[2] - state.targets[1]).To3d(), Color.white);

        //linear
        Vector3 force = acc * dirCurrent;
        //force -= force * Vector3.Dot(dir_current, rb.velocity) * (1 - Mathf.Exp(-rb.velocity.magnitude * LINEAR_DAMP));

        if (acc > 0.01)
        {
            Vector3 longitudinal_comp = dirCurrent * Vector3.Dot(dirCurrent, rb.velocity);
            Vector3 lateral_velocity = rb.velocity - longitudinal_comp;
            force -= lateralDamp * lateral_velocity;
        }

        //damp constant k = maxforce/maxspeed
        force -= (accelerationMax / velocityMax) * rb.velocity;

        rb.AddForce(force);

        //rotational
        if (dirInput.magnitude > 0.1)
            dirTarget = dirInput.normalized;

        float delta_angle = Vector3.SignedAngle(dirTarget, dirCurrent, -Vector3.up) / Mathf.PI;

        var sign = Mathf.Sign(Vector3.Dot(Vector3.Cross(dirCurrent, dirTarget), Vector3.up));
        delta_angle = Mathf.Acos(Vector3.Dot(dirTarget, dirCurrent)) * sign / Mathf.PI;

        pidController.ApplyTorque(dirCurrent, dirTarget);

    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0)
        {
            return;
        }
        var new_state = GetState();
        //TODO: calculate reward


        var moved = transform.position.xz() - lastPosition;
        lastPosition = transform.position.xz();

        var weigths = new float[3]; 
        
        for(int i = 0; i < weigths.Length; i++)
        {
            weigths[i] = 1.0f / (i + 1);
        }

        float reward = 0;
        for(int i = 0; i < weigths.Length; i++)
        {
            float distanceGained = state.targets[i].magnitude - (state.targets[i] - moved).magnitude;
            reward += weigths[i] * distanceGained;
        }
        reward = reward * 0.02f / (Time.deltaTime + 0.02f);
        totalReward += reward;

        bool finished = GetComponent<Racer>().HasFinished();
        Debug.Log(state);
        Debug.Log(takenAction);
        Debug.Log(reward);
        Debug.Log(new_state);
        Debug.Log(finished);



        DdpgTrainer.I.RememberAsync(state, takenAction, reward, new_state, finished);
        if(finished)
        {
            ResetAgent();
        }
    }

    DdpgState GetState()
    {
        DdpgState state = new DdpgState();
        state.velocity = rb.velocity.xz();
        state.forward = transform.forward.xz();
        state.angularVelocity = rb.angularVelocity.y;
        state.rayCastDistances = new float[numRays];
        state.targets = new Vector2[3];

        for (int i = 0; i < numRays; i++)
        {
            rays[i].origin = transform.position;
            rays[i].direction = Quaternion.Euler(0, i * 360.0f / numRays, 0) * Vector3.forward;
            RaycastHit hit;
            //Debug.DrawRay(rays[i].origin + rays[i].direction * 10, rays[i].direction * 100, Color.red);
            if (Physics.Raycast(rays[i], out hit, DdpgTrainer.I.rayCastDistance, raycastLayer, QueryTriggerInteraction.Ignore))
            {
                state.rayCastDistances[i] = hit.distance / DdpgTrainer.I.rayCastDistance;
            }
            else
            {
                state.rayCastDistances[i] = 1;
            }
        }

        var corners = path.corners;
        for(int i = 0; i < state.targets.Length; i++)
        {
            state.targets[i] = (corners[Math.Min(i + 1, corners.Length - 1)] - transform.position).xz();
        }

        return state;
    }
    
}
