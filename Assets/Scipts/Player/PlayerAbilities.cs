using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAbilities : MonoBehaviour
{
    public InputActionReference platformSpawing;
    public InputActionReference platformCalling;
    public InputActionReference movePlatform;
    public float countdown = 1f;
    public GameObject platformPrefab;
    public Vector2 spacing = new Vector2(3, 1);
    [Range(0.1f, 1f)] public float alpha = 0.5f;
    public GameObject prePlace;
    private float timer;
    private int pCount;
    public int pCountMax = 3;
    private List<GameObject> platforms = new List<GameObject>();

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
            timer = countdown;
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

    private void SpawnPrePlaceFalse()
    {
        if (prePlace != null)
        {
            MovePlatform();
            return;
        }

        float facing = GetComponent<PlayerMovement>().dir;
        Vector3 transformVector = transform.position + new Vector3(spacing.x, 0) * facing + new Vector3(0, spacing.y);
        prePlace = Instantiate(platformPrefab, transformVector, Quaternion.identity);
        prePlace.GetComponent<Collider2D>().enabled = false;
        SpriteRenderer rend = prePlace.GetComponent<SpriteRenderer>();
        rend.color = new Color(1, 0, 0, alpha);
        Platform pfP;
        if (prePlace.TryGetComponent<Platform>(out pfP))
        {
            pfP.enabled = false;
        }
    }

    private void MovePlatform()
    {
        Vector2 move = movePlatform.action.ReadValue<Vector2>();
        prePlace.transform.position += new Vector3(move.x, move.y);
    }

    private void SpawnPrePlace()
    {
        if (prePlace != null)
        {
            MovePlatform();
            return;
        }

        float facing = GetComponent<PlayerMovement>().dir;
        Vector3 transformVector = transform.position + new Vector3(spacing.x, 0) * facing + new Vector3(0, spacing.y);
        prePlace = Instantiate(platformPrefab, transformVector, Quaternion.identity);
        prePlace.GetComponent<Collider2D>().enabled = false;
        SpriteRenderer rend = prePlace.GetComponent<SpriteRenderer>();
        rend.color = new Color(rend.color.r, rend.color.g, rend.color.b, alpha);
        Platform pfP;
        if (prePlace.TryGetComponent<Platform>(out pfP))
        {
            pfP.enabled = false;
        }
    }

    private void SpawnPlatform()
    {
        Destroy(prePlace);
        float facing = GetComponent<PlayerMovement>().dir;
        Vector3 transformVector = transform.position + new Vector3(spacing.x, 0) * facing + new Vector3(0, spacing.y);
        GameObject pf = Instantiate(platformPrefab, transformVector, Quaternion.identity);
        platforms.Add(pf);
        pCount++;
    }
}