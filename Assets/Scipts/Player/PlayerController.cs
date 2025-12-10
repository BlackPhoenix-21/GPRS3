using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    private float health = 100;
    public float invincibleCooldown = 2f;
    private float invincibleTimer;

    void Update()
    {
        invincibleTimer -= Time.deltaTime;
    }

    public void TakeDamage(float damage)
    {
        if (invincibleTimer > 0)
            return;

        health -= damage;
        if (health <= 0)
            Death();
        invincibleTimer = invincibleCooldown;
    }

    private void Death()
    {
        throw new NotImplementedException();
    }
}
