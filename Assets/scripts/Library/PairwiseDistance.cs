using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class PairwiseDistance
{
    //List<List<float>> distances;
    List<List<float>> edges;

    List<List<float>> calculatedDistances;


    bool isDirty = true;

    //initialises distances with infinity
    public PairwiseDistance(int domainSize)
    {
        calculatedDistances = new List<List<float>>();
        edges = new List<List<float>>();
        for (int i = 0; i < domainSize; i++)
        {
            //distances.Add(new List<float>(domainSize - i));
            calculatedDistances.Add(new List<float>(domainSize - i));
            edges.Add(new List<float>(domainSize - i));
        }
        for (int i = 0; i < domainSize; i++)
        {
            for (int j = 0; j < domainSize - i; j++)
            {
                //distances[i].Add(Mathf.Infinity);
                calculatedDistances[i].Add(Mathf.Infinity);
                edges[i].Add(Mathf.Infinity);
            }
            edges[i][0] = 0;
        }
    }

    public void AddEdge(int idx1, int idx2, float dist)
    {
        lock(edges)
        {
            edges[Math.Min(idx1, idx2)][Math.Abs(idx2 - idx1)] = dist;
        }
        isDirty = true;
    }

/*    //sets the edge distances as the default (set using SetInitial())
    public void Reset()
    {
        for (int i = 0; i < distances.Count; i++)
        {
            for (int j = 0; j < distances.Count - i - 1; j++)
            {
                distances[i][j] = initEdges[i][j];
            }
        }
    }

    //the current edge distances are set as the default which are used when calling Reset()
    public void SetInitial()
    {
        //deep copy distances to initEdges 
        initEdges = new List<List<float>>();
        for (int i = 0; i < distances.Count; i++)
        {
            initEdges.Add(new List<float>());
            for (int j = 0; j < distances[i].Count; j++)
            {
                initEdges[i].Add(distances[i][j]);
            }
        }
    }
*/
    public float this[int idx1, int idx2]
    {
        get => GetDistance(idx1, idx2);
        //set => SetDistance(idx1, idx2, value);
    }

    float GetDistance(int idx1, int idx2)
    {
        if (isDirty)
        {
            Thread backgroundThread = new Thread(CalculateShortestDistanceImmediate);
            backgroundThread.Start();
            isDirty = false;
        }
        lock(calculatedDistances)
        {
            if(idx1 == idx2)
            {
                if (calculatedDistances[idx1][0] != 0)
                    Debug.LogWarning("bad impl");
            }

            return calculatedDistances[Math.Min(idx1, idx2)][Math.Abs(idx2 - idx1)];
        }
    }

/*    void SetDistance(int idx1, int idx2, float dist)
    {
        distances[Math.Min(idx1, idx2)][Math.Abs(idx2 - idx1)] = dist;
        isDirty = true;
    }*/

    public void CalculateShortestDistance()
    {
        if (isDirty)
        {
            Thread backgroundThread = new Thread(CalculateShortestDistanceImmediate);
            backgroundThread.Start();
        }
        else
        {
            Debug.LogWarning("Redundant call to CalculateShortestDistance");
        }
    }

    //TODO make this private once scene loading is implemented
    public void CalculateShortestDistanceImmediate()
    {
        //deep copy edge distances
        var dists = new List<List<float>>();
        lock (edges)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                dists.Add(new List<float>());
                for (int j = 0; j < edges[i].Count; j++)
                {
                    dists[i].Add(edges[i][j]);
                }
            }
            isDirty = false;
        }

        //Floyd-Warshall algorithm
        for (int k = 0; k < dists.Count; k++)
        {
            for (int i = 0; i < dists.Count; i++)
            {
                for (int j = 0; j < dists.Count; j++)
                {
                    if (dists[Math.Min(i, j)][Math.Abs(i - j)] >
                            dists[Math.Min(i, k)][Math.Abs(i - k)] +
                            dists[Math.Min(k, j)][Math.Abs(k - j)])

                        dists[Math.Min(i, j)][Math.Abs(i - j)] =
                            dists[Math.Min(i, k)][Math.Abs(i - k)] +
                            dists[Math.Min(k, j)][Math.Abs(k - j)];
                }
            }
        }
        lock(calculatedDistances)
        {
            calculatedDistances = dists;
        }
    }
}
