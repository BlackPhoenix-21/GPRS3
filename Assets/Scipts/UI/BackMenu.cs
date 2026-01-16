using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackMenu : MonoBehaviour
{
    public string menuSceneName = "MainMenu";

    public void BackToMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}
