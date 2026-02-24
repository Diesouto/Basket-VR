using System;
using UnityEngine;

public class PlayerChangeInteractors : MonoBehaviour
{
    [Header("Interactors")]
    [SerializeField] private MonoBehaviour uiInteractor;     // Interactor de la Interfaz (raycast)
    [SerializeField] private MonoBehaviour rightInteractor;  // Interactor de los elementos del juego

    private void Start()
    {
        // If there is no GameManager in scene → use UI interactor
        if (GameManager.Instance == null)
        {
            SetUIInteractorActive();
            return;
        }

        // Subscribe to state changes
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;

        // Set initial state
        UpdateInteractors();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= GameManager_OnStateChanged;
        }
    }

    private void GameManager_OnStateChanged(object sender, EventArgs e)
    {
        UpdateInteractors();
    }

    private void UpdateInteractors()
    {
        if (GameManager.Instance == null)
        {
            SetUIInteractorActive();
            return;
        }

        if (GameManager.Instance.IsGamePlaying())
        {
            SetRightInteractorActive();
        }
        else
        {
            SetUIInteractorActive();
        }
    }

    private void SetUIInteractorActive()
    {
        if (uiInteractor != null)
            uiInteractor.enabled = true;

        if (rightInteractor != null)
            rightInteractor.enabled = false;
    }

    private void SetRightInteractorActive()
    {
        if (uiInteractor != null)
            uiInteractor.enabled = false;

        if (rightInteractor != null)
            rightInteractor.enabled = true;
    }
}