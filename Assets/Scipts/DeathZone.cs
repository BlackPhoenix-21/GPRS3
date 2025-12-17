using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
       
        if (!other.CompareTag("Player"))
            return;
        else
        {
            
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
