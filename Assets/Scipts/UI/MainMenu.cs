using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Scene")]
    public string gameSceneName = "Level1";
    public GameObject won;

    public void Start()
    {
        won.SetActive(GameManager.Instance.won);
    }

    public void OnClickNewGame()
    {
        SceneManager.LoadScene(gameSceneName);
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
