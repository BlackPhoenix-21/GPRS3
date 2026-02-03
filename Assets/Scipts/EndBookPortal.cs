using UnityEngine;

public class EndBookPortal : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject thanksPanel;
    [SerializeField] private string playerTag = "Player";

    private bool activated;

    private void Awake()
    {
        activated = false;
    }

    public void ActivatePortal()
    {
        activated = true;
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!activated) return;
        if (!other.CompareTag(playerTag)) return;

        if (thanksPanel != null)
            thanksPanel.SetActive(true);

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
