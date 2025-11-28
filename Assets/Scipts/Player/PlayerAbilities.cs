using System;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{
    public KeyCode platformSpawing = KeyCode.E;
    public GameObject platformPrefab;
    public Vector2 spacing = new Vector2(3, 1);
    public GameObject prePlace;
    private GameObject platform;

    void Update()
    {

        if (platform == null)
        {
            if (Input.GetKeyUp(platformSpawing))
            {
                SpawnPlatform();
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
        SpriteRenderer r = prePlace.GetComponent<SpriteRenderer>();
        r.color = new Color(r.color.r, r.color.g, r.color.b, 125);
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