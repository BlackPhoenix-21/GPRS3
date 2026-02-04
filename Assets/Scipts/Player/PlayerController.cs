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
    public GameObject deathScreeen;

    private Image healthBar;
    private TMP_Text healthText;
    public GameObject UI;

    void Awake()
    {
        deathScreeen.SetActive(false);
        healthBar = GetComponentsInChildren<Image>().FirstOrDefault(x => x.name == "HB");
        healthText = GetComponentInChildren<TMP_Text>();
    }

    void Start()
    {
        health = GameManager.Instance.health;
        UI.SetActive(true);
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

    void Death()
    {
        deathScreeen.SetActive(true);
        UI.SetActive(false);
    }
}
