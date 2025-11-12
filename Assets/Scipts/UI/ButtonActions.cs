using UnityEngine;

public class ButtonActions : MonoBehaviour
{
    PauseController controller;

    private void Start()
    {
        controller = GameObject.Find("PauseController").GetComponent<PauseController>();
    }

    public void Pause()
    {
        controller.Pause();
    }

    public void Resume()
    {
        controller.Resume();
    }

    public void CloseOptions()
    {
        controller.CloseOptions();
    }

    public void OpenOptions()
    {
        controller.OpenOptions();
    }

    public void BackToMenu()
    {
        controller.BackToMenu();
    }
}
