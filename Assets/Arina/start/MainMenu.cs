using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Level1"; 

    [Header("Panels")]
    [SerializeField] private GameObject optionsPanel;

    [Header("Audio")]
    [SerializeField] private Slider volumeSlider;   // необязательно

    private void Start()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);

        // Загружаем сохранённую громкость (если хочешь)
        if (volumeSlider != null)
        {
            float v = PlayerPrefs.GetFloat("volume", 0.8f);
            volumeSlider.value = v;
            AudioListener.volume = v;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    public void OnClickPlay()
    {
        
        SceneManager.LoadScene(Level1);
    }

    public void OnClickOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void OnClickBackFromOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public void OnClickExit()
    {
        // В редакторе это не закроет Play Mode — сработает в билде
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnVolumeChanged(float v)
    {
        AudioListener.volume = v;
        PlayerPrefs.SetFloat("volume", v);
    }
}
