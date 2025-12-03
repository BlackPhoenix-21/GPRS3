using System;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{
    public KeyCode platformSpawing = KeyCode.E;
    public float countdown = 1f;
    public GameObject platformPrefab;
    public Vector2 spacing = new Vector2(3, 1);
    [Range(0.1f, 1f)] public float alpha = 0.5f;
    public GameObject prePlace;
    private GameObject platform;
    private float timer;

    void Update()
    {
        if (platform == null)
        {
            if (Input.GetKeyUp(platformSpawing) && timer < 0)
            {
                SpawnPlatform();
                timer = countdown;
            }
            else if (Input.GetKey(platformSpawing))
            {
                SpawnPrePlace();
            }
        }
        else
        {
            if (Input.GetKeyUp(platformSpawing))
            {
                Destroy(platform);
                platform = null;
            }
        }
        timer -= Time.deltaTime;
    }

    private void SpawnPrePlace()
    {
        if (prePlace != null)
        {
            Destroy(prePlace);
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
        platform = pf;
    }
}