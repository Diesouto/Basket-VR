using UnityEngine;

public class ButtonUI : MonoBehaviour
{
    public void ChangeSceneToMenu()
    {
        Loader.LoadScene(Loader.Scene.MainMenuSceneVR);
    }
    public void ChangeSceneToGameScene()
    {
        Loader.LoadScene(Loader.Scene.GameSceneVR);
    }
    public void ChangeSceneToHost()
    {
        Loader.LoadMultiplayer(true);
    }
    public void ChangeSceneToClient()
    {
        Loader.LoadMultiplayer(false);
    }
    public void QuitApplication()
    {
        Application.Quit();
    }
}
