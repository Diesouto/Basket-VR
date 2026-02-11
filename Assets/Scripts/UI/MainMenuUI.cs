using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] Button playButton;
    [SerializeField] Button hostButton;
    [SerializeField] Button clientButton;
    [SerializeField] Button quitButton;

    private void Awake()
    {
        // Nos aseguramos de que el juego no esté pausado al cargar el menú principal
        Time.timeScale = 1f;

        playButton.onClick.AddListener(() =>
        {
            Loader.LoadScene(Loader.Scene.GameScene);
        });
        hostButton.onClick.AddListener(() =>
        {
            Launcher.StartAsHost();
        });
        clientButton.onClick.AddListener(() =>
        {
            Launcher.StartAsClient();
        });
        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }
}
