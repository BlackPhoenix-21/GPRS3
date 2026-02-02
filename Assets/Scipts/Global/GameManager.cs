using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool isPaused = false;
    public float health = 100;
    public bool won;
    private float timer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // cutscene
    // tutorial
    // platform ui
    // 

    private void Update()
    {
        timer += Time.deltaTime;
    }
}

