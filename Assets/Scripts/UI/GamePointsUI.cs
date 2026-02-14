using TMPro;
using UnityEngine;
using System;
using Unity.Netcode;

public class GamePointsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI pointsText;

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
        if (pointsText == null) return;

        // Single-player fallback
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            int pts = 0;
            if (PointsManager.Instance != null) pts = PointsManager.Instance.GetPoints(0);
            pointsText.text = $"Points: {pts}";
            return;
        }

        ulong localId = NetworkManager.Singleton.LocalClientId;

        if (PointsManager.Instance == null)
        {
            pointsText.text = "Points: 0";
            return;
        }

        var all = PointsManager.Instance.GetAllPoints();

        int localPts = 0;
        all.TryGetValue(localId, out localPts);

        // If there are other clients, show their points too
        string display = $"You: {localPts}";
        foreach (var kv in all)
        {
            if (kv.Key == localId) continue;
            display += $"  |  Opponent({kv.Key}): {kv.Value}";
        }

        pointsText.text = display;
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