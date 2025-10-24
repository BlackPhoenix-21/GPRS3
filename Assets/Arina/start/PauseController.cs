using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    public static PauseController Instance { get; private set; }

    [Header("UI ( панель)")]
    [SerializeField] private GameObject pausePanel;

    [Header("Имя сцены с главным меню")]
    [SerializeField] private string menuSceneName = "start";

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);          // живём между сценами
        if (pausePanel != null) pausePanel.SetActive(false);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // На каждой сцене гарантируем нормальное время и скрытую панель
        Time.timeScale = 1f;
        IsPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void Update()
    {
        // В меню Esc не нужен
        if (SceneManager.GetActiveScene().name == menuSceneName) return;

        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause() { if (IsPaused) Resume(); else Pause(); }

    public void Pause()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
        IsPaused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        IsPaused = false;
    }

    public void BackToMenu()
    {
        Resume();
        SceneManager.LoadScene(menuSceneName);
    }
}
