using UnityEngine;
using UnityEngine.InputSystem;

public class VRPauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public InputActionProperty pauseButton;

    private bool menuOpened = false;

    void Update()
    {
        if (!menuOpened && pauseButton.action.WasPressedThisFrame())
        {
            OpenPauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        menuOpened = true;
    }
}
