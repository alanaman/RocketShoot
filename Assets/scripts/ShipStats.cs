using UnityEngine;

[CreateAssetMenu(fileName = "ShipStats", menuName = "RocketShoot/ShipStats")]
public class ShipStats : ScriptableObject
{
    public float mass;
    public float inertia;

    public float maxAcceleration1;
    public float maxAcceleration2;
    public float maxAcceleration3;
    public float maxVelocity;

    public float maxForce { get { return mass * maxAcceleration1; } }
    public float dampingConstant { get { return mass * maxAcceleration1/maxVelocity; } }

    public float turnSpeed;
    public float turnStability;

    public float lateralDamping;
}