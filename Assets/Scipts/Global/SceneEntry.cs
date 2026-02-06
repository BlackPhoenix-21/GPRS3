using UnityEngine;

public class SceneEntry : MonoBehaviour
{
    void Start()
    {
        Time.timeScale = 0;
    }

    public void DestroyAnim()
    {
        Time.timeScale = 1;
        GameObject.FindWithTag("Player").GetComponent<PlayerAbilities>().enabled = false;
        Destroy(gameObject);
    }
}
