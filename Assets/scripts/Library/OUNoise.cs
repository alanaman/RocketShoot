using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Tensorflow;
using Tensorflow.NumPy;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class OUNoise
{
    private int size;
    private float[] mu;
    private float theta;
    private float sigma;
    private float[] state;

    public OUNoise(int size, float mu = 0.0f, float theta = 0.15f, float sigma = 0.2f)
    {
        this.size = size;
        this.mu = (np.ones(size, TF_DataType.TF_FLOAT) * mu).ToArray<float>();
        this.theta = theta;
        this.sigma = sigma;
        Reset();
    }

    public void Reset()
    {
        state = new float[size];
        Array.Copy(mu, state, size);
    }

    public float[] Sample()
    {
        var x = np.array(state);
        var dx = (theta * (mu - x) + sigma * np.random.normal(size: size));
        state = (x + dx).ToArray<float>();
        return state;
    }
}
