using System;
using UnityEngine;

public class PointsManager : MonoBehaviour
{
    public static PointsManager Instance { get; private set; }

    public event EventHandler OnPointsChanged;

    [SerializeField] private int pointsPerBasket = 2;

    private int currentPoints = 0;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one PointsManager in scene!");
            return;
        }

        Instance = this;
    }

    public void AddBasketPoints()
    {
        currentPoints += pointsPerBasket;
        OnPointsChanged?.Invoke(this, EventArgs.Empty);

        Debug.Log("Total Points: " + currentPoints);
    }

    public int GetPoints()
    {
        return currentPoints;
    }

    public void ResetPoints()
    {
        currentPoints = 0;
        OnPointsChanged?.Invoke(this, EventArgs.Empty);
    }
}