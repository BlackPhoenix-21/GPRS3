using System;
using System.Collections.Generic;
using UnityEditor;
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
    public Vector2 deadzone = new Vector2(2.25f, 1.5f);
    public float speed = 1f;

    private GameObject prePlace;
    private float timer;
    private int pCount;
    private List<GameObject> platforms = new List<GameObject>();

    private Vector3 accPos;
    private Vector3 deadzonePos;
    private Vector3 deadzoneNeg;
    private float platDespawn = 5f;
    private List<float> pTimer = new List<float>();

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

    private void Start()
    {
        NewDeadzone();
    }

    private void NewDeadzone()
    {
        deadzonePos = transform.position + (Vector3)deadzone;
        deadzoneNeg = transform.position - (Vector3)deadzone;
    }

    private void Update()
    {
        if (platformSpawing.action.WasReleasedThisFrame() && timer < 0 && pCount < pCountMax)
        {
            SpawnPlatform();
            timer = cooldown;
        }
        else if (platformSpawing.action.IsPressed())
        {
            if (pCount >= pCountMax || timer > 0)
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

        pTimer.ForEach(t => { t -= Time.deltaTime; });
        pTimer.ForEach(t =>
        {
            if (t > 0)
            {
                Destroy(platforms[pTimer.IndexOf(t)]);
                platforms.RemoveAt(pTimer.IndexOf(t));
                pCount--;
            }
        });

        timer -= Time.deltaTime;
    }

    private void MovePlatform()
    {
        Vector2 moveValue = movePlatform.action.ReadValue<Vector2>();

        Vector3 move = new Vector3(moveValue.x, moveValue.y, 0) * speed * Time.deltaTime;
        NewDeadzone();
        Vector3 newPos = prePlace.transform.position + move;

        if (InDeadZone(newPos))
            prePlace.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, alpha);
        else
            prePlace.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);

        if (moveValue == Vector2.zero)
            return;

        prePlace.transform.position = DeadZone(newPos);
        accPos = prePlace.transform.position;
    }

    private Vector3 DeadZone(Vector3 pos)
    {
        Vector3 newPos = pos;
        if (InDeadZone(newPos))
        {
            // Position ist innerhalb der Deadzone - zur nächsten Kante schieben
            float distToRight = deadzonePos.x - newPos.x;
            float distToLeft = newPos.x - deadzoneNeg.x;
            float distToTop = deadzonePos.y - newPos.y;
            float distToBottom = newPos.y - deadzoneNeg.y;

            float minDist = Mathf.Min(distToRight, distToLeft, distToTop, distToBottom);

            if (minDist == distToRight)
                newPos.x = deadzonePos.x;
            else if (minDist == distToLeft)
                newPos.x = deadzoneNeg.x;
            else if (minDist == distToTop)
                newPos.y = deadzonePos.y;
            else
                newPos.y = deadzoneNeg.y;
        }
        return newPos;
    }

    private bool InDeadZone(Vector3 newPos)
    {
        if (newPos.x < deadzonePos.x && newPos.x > deadzoneNeg.x)
        {
            if (newPos.y < deadzonePos.y && newPos.y > deadzoneNeg.y)
            {
                return true;
            }
        }
        return false;
    }

    private GameObject PlatformSpawner()
    {
        float facing = GetComponent<PlayerMovement>().dir;
        Vector3 transformVector = transform.position + new Vector3(spacing.x, 0) * facing + new Vector3(0, spacing.y);
        return Instantiate(platformPrefab, transformVector, Quaternion.identity);
    }

    private void PrePlace(bool color)
    {
        prePlace.GetComponent<Collider2D>().enabled = false;

        if (!color)
            prePlace.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, alpha);
        else
            prePlace.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);

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
        PrePlace(true);
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
        PrePlace(false);
    }

    private void SpawnPlatform()
    {
        Destroy(prePlace);
        GameObject pf = PlatformSpawner();

        if (InDeadZone(accPos))
        {
            prePlace.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, alpha);
            Destroy(pf);
            return;
        }

        pf.transform.position = accPos;
        platforms.Add(pf);
        pTimer.Add(platDespawn);
        pCount++;
    }
}