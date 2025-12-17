using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyNPC : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float wallCheckDistance = 0.15f;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask obstacleMask;

    private Rigidbody2D rb;
    private Collider2D col;

    private int dir = -1;
    private bool activated;

    private readonly RaycastHit2D[] hits = new RaycastHit2D[8];
    private ContactFilter2D filter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = obstacleMask;
        filter.useTriggers = false;
    }

    private void Update()
    {
        if (activated) return;

        if (Camera.main == null) return;
        Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
        if (vp.z > 0f && vp.x >= 0f && vp.x <= 1f && vp.y >= 0f && vp.y <= 1f)
            activated = true;
    }

    [System.Obsolete]
    private void FixedUpdate()
    {
        if (!activated)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        if (IsWallAhead())
            Flip();

        rb.velocity = new Vector2(speed * dir, rb.velocity.y);
    }

    private bool IsWallAhead()
    {
        Vector2 origin = wallCheck ? (Vector2)wallCheck.position : (Vector2)transform.position;
        Vector2 castDir = Vector2.right * dir;

        int count = col.Cast(castDir, filter, hits, wallCheckDistance);

        for (int i = 0; i < count; i++)
        {
            var h = hits[i];
            if (h.collider == null) continue;
            if (h.collider == col) continue;

            if (h.collider.GetComponent<TurnObstacle2D>() == null) continue;

            Vector2 n = h.normal;
            if (Mathf.Abs(n.x) > 0.6f)
                return true;
        }

        return false;
    }

    private void Flip()
    {
        dir *= -1;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * dir;
        transform.localScale = s;
    }
}
