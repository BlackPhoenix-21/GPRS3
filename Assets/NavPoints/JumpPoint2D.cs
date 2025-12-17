using UnityEngine;

public class JumpPoint2D : MonoBehaviour
{
    public Transform landing;

    [Header("How to jump")]
    [Tooltip("How much higher than the higher point (start/landing) the arc apex should be.")]
    public float extraApexHeight = 1.5f;

    [Tooltip("Minimum flight time. Bigger = slower jump.")]
    public float minTime = 0.25f;

    [Tooltip("Maximum allowed horizontal speed. If too small, may not reach.")]
    public float maxHorizontalSpeed = 20f;

    public Vector2 CalculateVelocity(Vector2 startPos, Rigidbody2D rb)
    {
        if (landing == null) return Vector2.zero;

        Vector2 endPos = landing.position;

     
        float g = Physics2D.gravity.y * rb.gravityScale;

        float dx = endPos.x - startPos.x;
        float dy = endPos.y - startPos.y;

        float apexY = Mathf.Max(startPos.y, endPos.y) + extraApexHeight;

       
        float vy = Mathf.Sqrt(Mathf.Max(0.001f, 2f * (apexY - startPos.y) * -g));

        float tUp = vy / -g;
        float tDown = Mathf.Sqrt(Mathf.Max(0.001f, 2f * (apexY - endPos.y) / -g));

        float t = Mathf.Max(minTime, tUp + tDown);

        float vx = dx / t;
        vx = Mathf.Clamp(vx, -maxHorizontalSpeed, maxHorizontalSpeed);

        return new Vector2(vx, vy);
    }
}
