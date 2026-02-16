using System;
using UnityEngine;

public class WaitingForPlayersUI : MonoBehaviour
{
    void Start()
    {
        // Show the waiting UI initially
        gameObject.SetActive(true);

        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += OnStateChanged;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(object sender, EventArgs e)
    {
        // Hide when countdown / game starts, show otherwise
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.IsCountdownToStartActive() || GameManager.Instance.IsGamePlaying())
            gameObject.SetActive(false);
    }
}
