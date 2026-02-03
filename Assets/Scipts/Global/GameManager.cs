using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool isPaused = false;
    public float health = 100;
    public bool won;

    [Header("AFK timer")]
    public float maxTime = 60f;
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
        // Если игра выиграна (финал) — не надо выкидывать в меню по таймеру
        if (won) return;

        bool anyInput =
            (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
            (Mouse.current != null && (
                Mouse.current.leftButton.wasPressedThisFrame ||
                Mouse.current.rightButton.wasPressedThisFrame ||
                Mouse.current.middleButton.wasPressedThisFrame
            )) ||
            (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame);

        if (anyInput)
            timer = 0f;
        else
            timer += Time.deltaTime;

        if (timer > maxTime)
        {
            timer = 0f;
            SceneManager.LoadScene(0);
        }
    }

    public void OnSceneSwitch()
    {
        var p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            var pc = p.GetComponent<PlayerController>();
            if (pc != null) health = pc.health;
        }
    }

    public void ResetState()
    {
        won = false;
        timer = 0f;
        health = 100f;
    }
}
