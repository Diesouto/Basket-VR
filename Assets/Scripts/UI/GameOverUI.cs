using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerObtainedPointsText;
    [SerializeField] private TextMeshProUGUI rivalObtainedPointsText;
    [SerializeField] private Button returnToMainMenuButton;

    private void Start()
    {
        returnToMainMenuButton.onClick.AddListener(() =>
        {
            Launcher.ShutdownNetwork();
            SceneManager.LoadScene(Loader.Scene.MainMenuScene.ToString());
        });

        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        Hide();
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsGameOver())
        {
            if (Launcher.isMultiplayer)
            {
                MultiplayerGameOver();
            } else
            {
                SinglePlayerGameOver();
            }
        }
        else
        {
            Hide();
        }
    }

    private void SinglePlayerGameOver()
    {
        playerObtainedPointsText.text = PointsManager.Instance.GetPlayerPoints(0).ToString();
        Show();
    }

    private void MultiplayerGameOver()
    {
        playerObtainedPointsText.text = PointsManager.Instance.GetPlayerPoints().ToString();
        rivalObtainedPointsText.text = PointsManager.Instance.GetRivalPoints().ToString();

        Show();
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
