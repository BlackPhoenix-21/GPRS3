using UnityEngine;

public class DamageSource : MonoBehaviour
{
    private string targetTag = "Player";
    public float damage;
    public bool trigger = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (trigger)
            return;

        if (!collision.gameObject.CompareTag(targetTag))
            return;

        if (collision.gameObject.TryGetComponent<PlayerController>(out var playerController))
        {
            playerController.TakeDamage(damage);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!trigger)
            return;

        if (!collision.gameObject.CompareTag(targetTag))
            return;

        if (collision.gameObject.TryGetComponent<PlayerController>(out var playerController))
        {
            playerController.TakeDamage(damage);
        }
    }
}