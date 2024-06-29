using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackEdge : MonoBehaviour
{
    //[SerializeField] Transform root;
    [SerializeField] Transform left;
    [SerializeField] Transform right;
    [SerializeField] BoxCollider edgeCollider;

    [SerializeField] SkinnedMeshRenderer meshRenderer;
    public void Align(TrackCorner right, TrackCorner left)
    {
        Vector3 leftPos = left.transform.position;
        Vector3 rightPos = right.transform.position;
        transform.position = (leftPos + rightPos) /2;
        var direction = (leftPos - rightPos).normalized;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.AngleAxis(90, Vector3.up);

        this.left.position = left.GetRightEndpoint();
        this.right.position = right.GetLeftEndpoint();

        var size = edgeCollider.size;
        size.x = Vector3.Distance(leftPos, rightPos);
        edgeCollider.size = size;

        //rendering clipping bound
        Bounds bounds = meshRenderer.localBounds;
        size = bounds.size;
        size.x = Vector3.Distance(leftPos, rightPos);
        bounds.size = size;
        meshRenderer.localBounds = bounds;
    }
}
