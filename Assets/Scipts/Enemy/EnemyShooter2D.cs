using UnityEngine;

public class EnemyShooter2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";

    [Header("Shoot")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private EnemyBullet2D bulletPrefab;
    [SerializeField] private float fireRate = 2.0f;     
    [SerializeField] private float shootRange = 8f; 
    [SerializeField] private float minShootDistance = 1.2f; 

    [Header("Line of sight (optional)")]
    [SerializeField] private bool useLineOfSight = false;
    [SerializeField] private LayerMask obstacleLayer;  

    private float fireTimer;

    [System.Obsolete]
    private void Update()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
        }

        fireTimer -= Time.deltaTime;
        if (player == null || firePoint == null || bulletPrefab == null) return;

        float dx = player.position.x - transform.position.x;
        float dist = Mathf.Abs(dx);

        if (dist > shootRange) return;
        if (dist < minShootDistance) return;
        if (fireTimer > 0f) return;

        // опционально: проверка “вижу ли игрока”
        if (useLineOfSight)
        {
            Vector2 origin = firePoint.position;
            Vector2 dir = (player.position - firePoint.position).normalized;
            float checkDist = shootRange;

            RaycastHit2D hit = Physics2D.Raycast(origin, dir, checkDist, obstacleLayer);
            if (hit.collider != null)
            {
                return;
            }
        }

        Shoot(dx >= 0 ? Vector2.right : Vector2.left);
        fireTimer = fireRate;
    }

    [System.Obsolete]
    private void Shoot(Vector2 direction)
    {
        var bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.Init(direction);
    }
}
