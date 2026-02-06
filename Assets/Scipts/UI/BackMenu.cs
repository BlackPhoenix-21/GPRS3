using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackMenu : MonoBehaviour
{
    public string menuSceneName = "MainMenu";

    public void BackToMenu()
    {
        if (SceneManager.GetActiveScene().name == "BossLevel1")
            SceneManager.LoadScene("BossLevel1");
        else
            SceneManager.LoadScene(menuSceneName);
    }
}
