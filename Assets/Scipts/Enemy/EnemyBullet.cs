using UnityEngine;

public class EnemyBullet2D : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float damage = 10f;

    private Rigidbody2D rb;
    private float lifeTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    [System.Obsolete]
    public void Init(Vector2 direction)
    {
        lifeTimer = lifeTime;
        rb.velocity = direction * speed;
    }

    private void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<EnemyChaserHybrid2D>() != null)
        return;
       
        if (other.CompareTag("Player"))
        {
            var pc = other.GetComponentInParent<PlayerController>();
            if (pc != null) pc.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
            Destroy(gameObject);
    }
}
