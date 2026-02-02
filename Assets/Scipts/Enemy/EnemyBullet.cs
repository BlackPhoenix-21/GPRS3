using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyBullet2D : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float damage = 10f;

    private Rigidbody2D rb;
    private float timer;
    private GameObject owner;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        timer = lifeTime;
    }

    [System.Obsolete]
    public void Init(Vector2 direction, GameObject ownerObject)
    {
        owner = ownerObject;
        Vector2 dir = direction.normalized;
        rb.velocity = dir * speed;
        RotateToDirection(dir);
    }

    private void RotateToDirection(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == owner)
            return;

        if (other.CompareTag("Player"))
        {
            var pc = other.GetComponentInParent<PlayerController>();
            if (pc != null)
                pc.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }
        if (!other.isTrigger)
            Destroy(gameObject);
    }
}
