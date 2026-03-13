using System;
using System.Collections;
using UnityEngine;

public class PlayerChangeInteractors : MonoBehaviour
{
    [Header("Interactors")]
    [SerializeField] private MonoBehaviour uiInteractor;     // Interactor de la Interfaz (raycast)
    [SerializeField] private MonoBehaviour rightInteractor;  // Interactor de los elementos del juego

    private void Start()
    {
        StartCoroutine(WaitForGameManager());
    }

    private IEnumerator WaitForGameManager()
    {
        while (GameManager.Instance == null)
            yield return null;

        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;

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
        if (GameManager.Instance.IsGamePlaying())
        {
            uiInteractor.gameObject.SetActive(false);
            rightInteractor.gameObject.SetActive(true);
        }
        else
        {
            uiInteractor.gameObject.SetActive(true);
            rightInteractor.gameObject.SetActive(false);
        }
    }
}