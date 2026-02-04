using UnityEngine;

public class EnemyShooter2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";

    [Header("Shoot")]
    [SerializeField] private EnemyBullet2D bulletPrefab;
    [SerializeField] private float fireRate = 1.2f;
    [SerializeField] private float shootRange = 8f;
    [SerializeField] private float minShootDistance = 1.2f;

    [Header("Spawn offset")]
    [SerializeField] private Vector2 shootOffset = new Vector2(0.6f, 0.2f);

    [Header("Visual")]
    [SerializeField] private SpriteRenderer sprite;

    private float fireTimer;

    private void Awake()
    {
        if (sprite == null)
            sprite = GetComponentInChildren<SpriteRenderer>();
    }

    [System.Obsolete]
    private void Update()
    {
        // найти игрока
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
        }

        fireTimer -= Time.deltaTime;
        if (player == null || bulletPrefab == null) return;

        Vector2 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;

        if (dist > shootRange) return;
        if (dist < minShootDistance) return;
        if (fireTimer > 0f) return;

        // направление в игрока (включая вверх/вниз)
        Vector2 direction = toPlayer.normalized;

        // флип врага
        if (sprite != null)
            sprite.flipX = direction.x < 0;

        Shoot(direction);
        fireTimer = fireRate;
    }

    [System.Obsolete]
    private void Shoot(Vector2 direction)
    {
        Vector3 spawnPos = transform.position;

        // если игрок почти прямо сверху / снизу
        float xSign = Mathf.Abs(direction.x) < 0.05f
            ? (sprite != null && sprite.flipX ? -1f : 1f)
            : Mathf.Sign(direction.x);

        spawnPos.x += shootOffset.x * xSign;
        spawnPos.y += shootOffset.y;

        EnemyBullet2D bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        bullet.Init(direction, gameObject);
    }
}
