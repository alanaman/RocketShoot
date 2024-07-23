using System;
using static Tensorflow.Binding;
using Tensorflow;
using System.Text;
using UnityEngine;

namespace Ddpg
{
    public struct DdpgAction
    {
        public float Xaxis;
        public float Yaxis;
        public float acc;

        public static int Size
        {
            get
            {
                return 3;
            }
        }
        public DdpgAction(float Xaxis, float Yaxis, float acc)
        {
            this.Xaxis = Xaxis;
            this.Yaxis = Yaxis;
            this.acc = acc;
        }

        public DdpgAction(float[] data)
        {
            if (data.Length != Size)
            {
                throw new ArgumentException("Invalid data size for Action");
            }
            Xaxis = data[0];
            Yaxis = data[1];
            acc = data[2];
        }

        public Vector2 GetDirectionVector()
        {
            return (new Vector2(Xaxis, Yaxis)).normalized;
        }

        public void AddNoise(float[] noise, float t)
        {
            Xaxis = Mathf.Lerp(Xaxis, noise[0], t);
            Yaxis = Mathf.Lerp(Yaxis, noise[1], t);
            acc = Mathf.Lerp(acc, noise[2], t);
        }

        public void Clip()
        {
            Xaxis = Mathf.Clamp(Xaxis, -1, 1);
            Yaxis = Mathf.Clamp(Yaxis, -1, 1);
            acc = Mathf.Clamp(acc, 0, 1);
        }

        public readonly float[] GetData()
        {
            var data = new float[3];
            data[0] = Xaxis;
            data[1] = Yaxis;
            data[2] = acc;
            return data;
        }
        public Tensor ToTensor()
        {
            return tf.constant(GetData());
        }

        public override string ToString()
        {
            return "Xaxis: " + Xaxis + ", Yaxis: " + Yaxis + ", acc: " + acc;
        }
    }

    public struct DdpgState
    {
        public Vector2 forward;
        public Vector2 velocity;
        public float angularVelocity;
        public float[] rayCastDistances;
        public Vector2[] targets;
        //public Vector2[] nearestRivals;

        public static int Size
        {
            get
            {
                return 5 + 8 + 2 * 3;
            }
        }

        public DdpgState(float[] data)
        {
            if (data.Length != Size)
            {
                throw new ArgumentException("Invalid data size for State");
            }
            int idx = 0;
            forward = new Vector2(data[idx++], data[idx++]);
            velocity = new Vector2(data[idx++], data[idx++]);
            angularVelocity = data[idx++];
            rayCastDistances = new float[8];
            Array.Copy(data, idx, rayCastDistances, 0, 8);
            idx += 8;
            targets = new Vector2[3];
            for (int i = 0; i < 3; i++)
            {
                targets[i] = new Vector2(data[idx++], data[idx++]);
            }
        }

        public readonly float[] GetData()
        {
            int dataSize = 5 + rayCastDistances.Length + 2 * targets.Length;
            var data = new float[dataSize];
            int idx = 0;

            data[idx++] = forward.x;
            data[idx++] = forward.y;
            data[idx++] = velocity.x;
            data[idx++] = velocity.y;
            data[idx++] = angularVelocity;

            Array.Copy(rayCastDistances, 0, data, idx, rayCastDistances.Length);
            idx += rayCastDistances.Length;

            for (int i = 0; i < targets.Length; i++)
            {
                data[idx++] = targets[i].x;
                data[idx++] = targets[i].y;
            }

            //for (int i = 0; i < nearestRivals.Length; i++)
            //{
            //    data[idx++] = nearestRivals[i].x;
            //    data[idx++] = nearestRivals[i].y;
            //}

            return data;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Forward: ");
            sb.Append(forward);
            sb.Append(", Velocity: ");
            sb.Append(velocity);
            sb.Append(", AngularVelocity: ");
            sb.Append(angularVelocity);
            sb.Append(", RayCastDistances: ");
            sb.Append("[");
            for (int i = 0; i < rayCastDistances.Length; i++)
            {
                sb.Append(rayCastDistances[i]);
                if (i < rayCastDistances.Length - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append("]");
            sb.Append(", Targets: ");
            sb.Append("[");
            for (int i = 0; i < targets.Length; i++)
            {
                sb.Append(targets[i]);
                if (i < targets.Length - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append("]");
            return sb.ToString();
        }

        public Tensor ToTensor()
        {
            return tf.constant(GetData());
        }
    }

}