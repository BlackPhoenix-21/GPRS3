using UnityEngine;

public class DamageSource : MonoBehaviour
{
    private string targetTag = "Player";   

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag(targetTag))
            return;

        if (collision.gameObject.TryGetComponent<PlayerController>(out var playerController))
        {
            playerController.TakeDamage();
        }
    }

}