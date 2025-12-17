using UnityEngine;

public class PathPoint2D : MonoBehaviour
{
    public PathPoint2D next;
    public bool isJumpStart;
    public Transform landing;
    public float upVelocity = 12f;
    public float forwardVelocity = 8f;
    public float arriveDistance = 0.25f;
}
