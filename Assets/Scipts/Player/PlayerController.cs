using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    public float invincibleCooldown = 2f;

    [HideInInspector] public float health = 100;
    private float invincibleTimer;

    private Image healthBar;
    private TMP_Text healthText;

    void Start()
    {
        health = GameManager.Instance.health;
        healthBar = GetComponentsInChildren<Image>().FirstOrDefault(x => x.name == "HB");
        healthText = GetComponent<TMP_Text>();
    }

    void Update()
    {
        invincibleTimer -= Time.deltaTime;
        healthBar.fillAmount = health / 100;
        healthText.text = health.ToString() + " / 100";
    }

    public void TakeDamage(float damage)
    {
        if (invincibleTimer > 0)
            return;

        health -= damage;
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
