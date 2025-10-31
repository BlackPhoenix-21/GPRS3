using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Level1";

    public void OnClickPlay()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnClickOptions()
    {
        GameObject.Find("PauseController").GetComponent<PauseController>().OpenOptions();
    }

    public void OnClickExit()
    {
        Application.Quit();
    }
}
