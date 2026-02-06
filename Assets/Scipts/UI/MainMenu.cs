using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene")]
    public string gameSceneName = "Level1";

    public void OnClickNewGame()
    {
        GameManager.Instance.StartCoro();
    }

    public void OnClickContinue()
    {
        // IDK
        print("Game");
    }

    public void OnClickOptions()
    {
        GameObject.Find("PauseController").GetComponent<PauseController>().OpenOptions();
    }

    public void OnClickExit()
    {
        Application.Quit();
    }

    public void OnClickDestory(GameObject go)
    {
        Destroy(go);
    }
}
