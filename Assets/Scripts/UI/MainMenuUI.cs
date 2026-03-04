using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] Button playButton;
    [SerializeField] Button hostButton;
    [SerializeField] Button clientButton;
    [SerializeField] TMP_InputField hostCodeInput;
    [SerializeField] Button quitButton;

    private void Awake()
    {
        // Nos aseguramos de que el juego no est� pausado al cargar el men� principal
        Time.timeScale = 1f;

        playButton.onClick.AddListener(() =>
        {
            Loader.LoadScene(Loader.Scene.GameSceneVR);
        });

        hostButton.onClick.AddListener(() =>
        {
            Loader.LoadMultiplayer(true);
        });

        clientButton.onClick.AddListener(() =>
        {
            string joinCode = hostCodeInput != null ? hostCodeInput.text.Trim() : "";
            Loader.LoadMultiplayer(false, joinCode);
        });

        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }
}
