using TMPro;
using UnityEngine;
using System;

public class GamePointsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI pointsText;

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        PointsManager.Instance.OnPointsChanged += PointsManager_OnPointsChanged;

        Hide();
    }

    private void PointsManager_OnPointsChanged(object sender, EventArgs e)
    {
        UpdatePoints();
    }

    private void UpdatePoints()
    {
        pointsText.text = "Points: " + PointsManager.Instance.GetPoints();
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