using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishZone : MonoBehaviour
{
    public static bool Finished;
    public string bossLevel = "BossLevel1";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Finished = true;
            GameManager.Instance.health = GameObject.FindWithTag("Player").GetComponent<PlayerController>().health;
            SceneManager.LoadScene(bossLevel);
        }
    }
}

