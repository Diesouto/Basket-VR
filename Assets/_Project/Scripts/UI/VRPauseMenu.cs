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
        menuOpened = !menuOpened;
        pauseMenu.gameObject.SetActive(menuOpened);
    }
}
