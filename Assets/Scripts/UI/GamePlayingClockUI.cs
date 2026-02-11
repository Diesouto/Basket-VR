using UnityEngine;
using UnityEngine.UI;

public class GamePlayingClockUI : MonoBehaviour
{
    [SerializeField] Image timerImage;

    private void Update()
    {
        timerImage.fillAmount = GameManager.Instance.GetGamePlayingTimerNormalized();
    }
}
