using UnityEngine;
using UnityEngine.InputSystem;

public class VRPauseMenu : MonoBehaviour
{
    [SerializeField] Canvas pauseMenu;
    [SerializeField] InputActionReference pauseButton;

    bool menuOpened = false;

    private void Start()
    {
        pauseButton.action.performed += PausePerformed;
    }

    private void PausePerformed(InputAction.CallbackContext obj)
    {
        TogglePauseMenu();
    }

    public void TogglePauseMenu()
    {
        menuOpened = !menuOpened;
        gameObject.GetComponent<PlayerChangeInteractors>().ToggleInteractors(menuOpened);
        pauseMenu.gameObject.SetActive(menuOpened);
    }
}
