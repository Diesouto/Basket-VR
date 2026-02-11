using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
            obtainedPointsText.text = PointsManager.Instance.GetPoints().ToString();
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
