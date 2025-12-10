using UnityEngine;

public class DamageSource : MonoBehaviour
{
    public new TagHandle tag;
    public float damage;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(tag))
        {
            if (TryGetComponent<PlayerController>(out var playerController))
            {
                playerController.TakeDamage(damage);
            }
        }
    }
}