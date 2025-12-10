using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
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

    public void TakeDamage()
    {
        if (invincibleTimer > 0)
            return;

        health--;
        if (health <= 0)
            Death();
        invincibleTimer = invincibleCooldown;
         Debug.Log("Player bekommt Schaden. Health: " + health);
    }

    private void Death()
    {
        throw new NotImplementedException();
    }
}
