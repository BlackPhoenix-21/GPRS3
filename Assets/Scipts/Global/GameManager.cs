using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool isPaused = false;
    public float health = 100;
    public bool won;
    public float maxTime = 60;
    public float timer;

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

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame ||
            Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame ||
            Mouse.current.rightButton.wasPressedThisFrame ||
            Mouse.current.middleButton.wasPressedThisFrame) ||
            Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame
            && Keyboard.current.anyKey.isPressed)
        {
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }

        if (timer > maxTime)
        {
            timer = 0;
            SceneManager.LoadScene(0);
        }
    }

    public void OnSceneSwitch()
    {
        health = GameObject.FindWithTag("Player").GetComponent<PlayerController>().health;
    }
}

