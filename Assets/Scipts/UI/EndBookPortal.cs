using System;
using System.Collections;
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
        StopCoroutine(Finished());
        if (!activated) return;
        if (!other.CompareTag(playerTag)) return;

        if (thanksPanel != null)
            thanksPanel.SetActive(true);

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        StopCoroutine(Finished());
        if (!activated) return;
        if (!other.CompareTag(playerTag)) return;

        if (thanksPanel != null)
            thanksPanel.SetActive(true);

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public IEnumerator Finished()
    {
        yield return new WaitForSeconds(5f);

        if (thanksPanel != null)
            thanksPanel.SetActive(true);

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
