using TMPro;
using UnityEngine;

public class GameJoinCodeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI joinCodeText;

    private void Start()
    {
        // Check if Launcher instance exists
        var launcher = FindFirstObjectByType<Launcher>();
        if (launcher != null && !string.IsNullOrEmpty(launcher.CurrentJoinCode))
        {
            joinCodeText.text = launcher.CurrentJoinCode;
        }
        else
        {
            joinCodeText.text = "(not generated yet)";
        }
    }

    private void Update()
    {
        if (GameManager.Instance.IsCountdownToStartActive())
        {
            gameObject.SetActive(false);
        }
    }
}