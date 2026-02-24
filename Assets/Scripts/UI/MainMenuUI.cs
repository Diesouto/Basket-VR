using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] Button playButton;
    [SerializeField] Button hostButton;
    [SerializeField] Button clientButton;
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
            Loader.LoadMultiplayer(false);
        });

        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }
}
