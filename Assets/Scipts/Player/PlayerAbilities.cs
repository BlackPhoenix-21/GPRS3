using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAbilities : MonoBehaviour
{
    [Header("Inputs")]
    public InputActionReference platformSpawing;
    public InputActionReference platformCalling;
    public InputActionReference movePlatform;
    [Header("Platform")]
    public float cooldown = 1f;
    public GameObject platformPrefab;
    [Tooltip("Max Platform Count")]
    public int pCountMax = 3;
    [Range(0.1f, 1f)] public float alpha = 0.5f;
    [Header("Moving")]
    public Vector2 spacing = new Vector2(3, 0);
    public Vector2 deadzone = new Vector2(2, 1);
    public float speed = 1f;

    private GameObject prePlace;
    private float timer;
    private int pCount;
    private List<GameObject> platforms = new List<GameObject>();
    private Vector3 accPos;

    private void OnEnable()
    {
        platformCalling.action.Enable();
        platformSpawing.action.Enable();
        movePlatform.action.Enable();
    }

    private void OnDisable()
    {
        platformCalling.action.Disable();
        platformSpawing.action.Disable();
        movePlatform.action.Disable();
    }

    void Update()
    {
        if (platformSpawing.action.WasReleasedThisFrame() && timer < 0 && pCount < pCountMax)
        {
            SpawnPlatform();
            timer = cooldown;
        }
        else if (platformSpawing.action.IsPressed())
        {
            if (pCount >= pCountMax)
                SpawnPrePlaceFalse();
            else
                SpawnPrePlace();
            return;
        }
        else
        {
            if (prePlace != null)
            {
                Destroy(prePlace);
                prePlace = null;
            }
        }

        if (platformCalling.action.WasPressedThisFrame() && pCount > 0)
        {
            Destroy(platforms[0]);
            platforms.RemoveAt(0);
            pCount--;
        }

        timer -= Time.deltaTime;
    }

    private void MovePlatform()
    {
        Vector2 moveValue = movePlatform.action.ReadValue<Vector2>();
        Vector3 move = new Vector3(moveValue.x, moveValue.y, 0) * speed * Time.deltaTime;

        Vector3 newPos = prePlace.transform.position + move;
        Vector3 localPos = newPos - transform.position;

        bool wouldBeInsideDeadzone = Mathf.Abs(localPos.x) <= deadzone.x && Mathf.Abs(localPos.y) <= deadzone.y;

        if (wouldBeInsideDeadzone)
        {
            float pushX = localPos.x;
            float pushY = localPos.y;

            if (Mathf.Abs(pushX) <= deadzone.x)
            {
                pushX = Mathf.Sign(pushX) * deadzone.x;
            }

            if (Mathf.Abs(pushY) <= deadzone.y)
            {
                pushY = Mathf.Sign(pushY) * deadzone.y;
            }

            prePlace.transform.position = transform.position + new Vector3(pushX, pushY, 0);
        }
        else
        {
            prePlace.transform.position = newPos;
        }

        accPos = prePlace.transform.position;
    }

    private GameObject PlatformSpawner()
    {
        float facing = GetComponent<PlayerMovement>().dir;
        Vector3 transformVector = transform.position + new Vector3(spacing.x, 0) * facing + new Vector3(0, spacing.y);
        return Instantiate(platformPrefab, transformVector, Quaternion.identity);
    }

    private void PrePlace(GameObject pl, bool color)
    {
        prePlace.GetComponent<Collider2D>().enabled = false;
        SpriteRenderer rend = prePlace.GetComponent<SpriteRenderer>();

        if (!color)
            rend.color = new Color(1, 0, 0, alpha);
        else
            rend.color = new Color(rend.color.r, rend.color.g, rend.color.b, alpha);

        Platform pfP;
        if (prePlace.TryGetComponent<Platform>(out pfP))
        {
            pfP.enabled = false;
        }
    }

    private void SpawnPrePlace()
    {
        if (prePlace != null)
        {
            MovePlatform();
            return;
        }
        prePlace = PlatformSpawner();
        accPos = prePlace.transform.position;
        PrePlace(prePlace, true);
    }

    private void SpawnPrePlaceFalse()
    {
        if (prePlace != null)
        {
            MovePlatform();
            return;
        }

        prePlace = PlatformSpawner();
        accPos = prePlace.transform.position;
        PrePlace(prePlace, false);
    }

    private void SpawnPlatform()
    {
        Destroy(prePlace);
        GameObject pf = PlatformSpawner();
        pf.transform.position = accPos;
        platforms.Add(pf);
        pCount++;
    }
}