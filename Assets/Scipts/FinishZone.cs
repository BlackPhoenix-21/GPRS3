using UnityEngine;

public class FinishZone : MonoBehaviour
{
    public static bool Finished;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            Finished = true;
    }
}

