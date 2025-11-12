using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;              // мяч
    public Rigidbody2D targetRb;          // (опц.) для look-ahead от скорости

    [Header("Follow")]
    public Vector2 offset = new Vector2(2f, 1f); // сдвиг относительно мяча
    public float smoothTime = 0.2f;              // сглаживание (меньше = быстрее)
    public float lookAheadScale = 0.25f;         // упреждение по скорости

    [Header("Bounds (optional)")]
    public bool useBounds = false;
    public Collider2D levelBounds;               // BoxCollider2D с границами уровня
    public float camPadding = 0.5f;              // запас от краёв

    Vector3 _vel; // вспомогательный для SmoothDamp
    Camera _cam;

    void Awake() => _cam = GetComponent<Camera>();

    void LateUpdate()
    {
        if (!target) return;

        // базовая цель + смещение
        Vector3 desired = target.position + (Vector3)offset;
        desired.z = transform.position.z; // фиксируем Z

        // look-ahead по скорости, если указан Rigidbody2D
        if (targetRb)
        {
            Vector2 v = targetRb.linearVelocity; 
            desired += (Vector3)(v * lookAheadScale);
        }

        // плавно едем к цели
        Vector3 next = Vector3.SmoothDamp(transform.position, desired, ref _vel, smoothTime);

        // ограничение по границам уровня
        if (useBounds && levelBounds)
            next = ClampToBounds(next);

        transform.position = next;
    }

    Vector3 ClampToBounds(Vector3 pos)
    {
        Bounds b = levelBounds.bounds;

        // учтём размер видимой области камеры (ортографической)
        float vertExtent = _cam.orthographicSize;
        float horzExtent = vertExtent * _cam.aspect;

        float minX = b.min.x + horzExtent + camPadding;
        float maxX = b.max.x - horzExtent - camPadding;
        float minY = b.min.y + vertExtent + camPadding;
        float maxY = b.max.y - vertExtent - camPadding;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        return pos;
    }
}
