using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    private int health = 2;
    public float invincibleCooldown = 2f;
    private float invincibleTimer;

    void Update()
    {
        invincibleTimer -= Time.deltaTime;
    }

    public void TakeDamge()
    {
        if (invincibleTimer > 0)
            return;

        health--;
        if (health <= 0)
            Death();
        invincibleTimer = invincibleCooldown;
    }

    private void Death()
    {
        throw new NotImplementedException();
    }
}
