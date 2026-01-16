using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseController : MonoBehaviour
{
    public static PauseController Instance { get; private set; }

    [Header("UI (панель)")]
    [Tooltip("Prefab for PauseMenu")]
    public GameObject pausePanelPrefab;
    [Tooltip("Prefab for OptionsMenu")]
    public GameObject optionsPanelPrefab;
    private GameObject pausePanel;
    private GameObject optionsPanel;

    private bool options = false;

    [Header("Audio")]
    [Tooltip("Sets Automatic on Start")]
    public Slider volumeSlider;

    [Header("Menu (Имя сцены с главным меню)")]
    public string menuSceneName = "MainMenu";

    public bool once = false;

    private void Awake()
    {
        if (once)
            return;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (once)
            return;

        if (SceneManager.GetActiveScene().name == menuSceneName)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    private void OnVolumeChanged(float v)
    {
        AudioListener.volume = v;
        PlayerPrefs.SetFloat("volume", v);
    }

    public void OptionsMenu()
    {
        if (options)
            OpenOptions();
        else
            CloseOptions();
        options = !options;
    }

    public void CloseOptions()
    {
        Destroy(optionsPanel);
    }

    public void OpenOptions()
    {
        optionsPanel = Instantiate(optionsPanelPrefab);
        volumeSlider = optionsPanel.GetComponentInChildren<Slider>();
        float v = PlayerPrefs.GetFloat("volume", 0.8f);
        volumeSlider.value = v;
        AudioListener.volume = v;
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    public void TogglePause()
    {
        if (GameManager.Instance.isPaused)
            Resume();
        else
            Pause();
        GameManager.Instance.isPaused = !GameManager.Instance.isPaused;
    }

    public void Pause()
    {
        pausePanel = Instantiate(pausePanelPrefab);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        Destroy(pausePanel);
        Time.timeScale = 1f;
    }

    public void BackToMenu()
    {
        if (!once)
        {
            Resume();
            GameManager.Instance.isPaused = false;
        }
        SceneManager.LoadScene(menuSceneName);
    }
}