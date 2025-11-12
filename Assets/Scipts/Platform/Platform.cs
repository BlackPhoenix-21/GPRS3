using System.Collections;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class Platform : MonoBehaviour
{
    public enum Mode { None, Break, SlowPlayer }
    public Mode mode = Mode.None;

    [Header("Break")]
    public float breakDelay = 3f;
    public float cycleTime = 0.1f;
    public float destroyDelay = 5f;
    public Color warnColor = Color.red;

    [Header("SlowPlayer")]
    [Range(0.1f, 1f)] public float slowMultiplier = 0.5f;
    public float slowDuration = 1.5f;

    private SpriteRenderer spriteR;
    private Rigidbody2D rbPlatform;
    private bool busy;
    private float runSpeed;
    private float dashSpeed;

    [Header("Auto Layer")]
    public bool autoSetGroundLayer = true;
    public LayerMask groundLayer;

    private void OnValidate()
    {
        if (autoSetGroundLayer)
        {
            int layerIndex = (int)Mathf.Log(groundLayer.value, 2);
            gameObject.layer = layerIndex;
        }
    }

    private void Awake()
    {
        if (mode == Mode.None)
        {
            Debug.LogWarning($"{gameObject.name} has no Platformmode!");
            return;
        }

        spriteR = GetComponent<SpriteRenderer>();

        rbPlatform = GetComponent<Rigidbody2D>();
        rbPlatform.bodyType = RigidbodyType2D.Static;

        if (gameObject.layer == 0)
            Debug.LogWarning($"{gameObject.name} has no LayerMask");
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (!c.collider.CompareTag("Player"))
            return;

        if (mode == Mode.Break && !busy)
        {
            StartCoroutine(BreakRoutine());
        }
        else if (mode == Mode.SlowPlayer)
        {
            PlayerMovement pM = c.gameObject.GetComponent<PlayerMovement>();
            runSpeed = pM.runSpeed;
            dashSpeed = pM.dashSpeed;
            pM.runSpeed *= slowMultiplier;
            pM.dashSpeed *= slowMultiplier;
        }
    }

    private void OnCollisionExit2D(Collision2D c)
    {
        if (mode == Mode.SlowPlayer)
        {
            // Vllt kurzer delay?
            PlayerMovement pM = c.gameObject.GetComponent<PlayerMovement>();
            pM.runSpeed = runSpeed;
            pM.dashSpeed = dashSpeed;
        }
    }

    private IEnumerator BreakRoutine()
    {
        busy = true;

        float t = breakDelay;
        while (t > 0)
        {
            float k = Mathf.PingPong(Time.time * 6f, 1f);
            spriteR.color = Color.Lerp(spriteR.color, warnColor, k);
            yield return new WaitForSeconds(0.1f);
            t -= k;
        }

        rbPlatform.bodyType = RigidbodyType2D.Dynamic;
        StartCoroutine(BreakCollider(0.5f));

        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }

    private IEnumerator BreakCollider(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.GetComponent<Collider2D>().enabled = false;
    }
}
