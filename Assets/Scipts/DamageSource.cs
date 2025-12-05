using UnityEngine;

public class DamageSource : MonoBehaviour
{
    public new TagHandle tag;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(tag))
        {
            if (TryGetComponent<PlayerController>(out var playerController))
            {
                playerController.TakeDamge();
            }
        }
    }
}