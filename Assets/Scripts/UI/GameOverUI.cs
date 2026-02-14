using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;

public class GameOver : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI obtainedPointsText;
    [SerializeField] private Button returnToMainMenuButton;

    private void Start()
    {
        returnToMainMenuButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(Loader.Scene.MainMenuScene.ToString());
        });

        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        Hide();
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsGameOver())
        {
            int pts = 0;
            if (PointsManager.Instance != null)
            {
                if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                    pts = PointsManager.Instance.GetPoints(NetworkManager.Singleton.LocalClientId);
                else
                    pts = PointsManager.Instance.GetPoints(0);
            }
            obtainedPointsText.text = pts.ToString();
            Show();
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
