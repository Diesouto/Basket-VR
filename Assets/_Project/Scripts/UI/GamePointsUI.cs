using TMPro;
using UnityEngine;
using System;
using Unity.Netcode;

public class GamePointsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerPointsText;
    [SerializeField] private TextMeshProUGUI rivalPointsText;

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;

        if (PointsManager.Instance != null)
            PointsManager.Instance.OnPointsChanged += PointsManager_OnPointsChanged;

        Hide();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= GameManager_OnStateChanged;

        if (PointsManager.Instance != null)
            PointsManager.Instance.OnPointsChanged -= PointsManager_OnPointsChanged;
    }

    private void PointsManager_OnPointsChanged(object sender, EventArgs e)
    {
        UpdatePoints();
    }

    private void UpdatePoints()
    {
        if (playerPointsText == null) return;

        // Single-player fallback
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            int pts = 0;
            if (PointsManager.Instance != null) pts = PointsManager.Instance.GetPlayerPoints(0);
            playerPointsText.text = $"Points: {pts}";
            return;
        }

        playerPointsText.text = $"You: {PointsManager.Instance.GetPlayerPoints().ToString()}";
        rivalPointsText.text = $"Rival: {PointsManager.Instance.GetRivalPoints().ToString()}";
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsGamePlaying())
        {
            Show();
            UpdatePoints();
        }
        else
        {
            Hide();
        }
    }

    void Show()
    {
        gameObject.SetActive(true);
    }

    void Hide()
    {
        gameObject.SetActive(false);
    }
}