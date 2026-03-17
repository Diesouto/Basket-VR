using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    public GameObject optionsPanel;
    public GameObject leaderboardPanel;

    // Abrir panel de opciones
    public void OpenOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }

    // Abrir leaderboard
    public void OpenLeaderBoard()
    {
        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(true);
    }

    // Cerrar panel actual
    public void Close()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);
    }

    // Volver al menú principal
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenuSceneVR");
    }
}
