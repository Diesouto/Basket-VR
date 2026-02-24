using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene
    {
        MainMenuSceneVR,
        GameSceneVR,
        MultiplayerSceneVR,
        LoadingScene
    }

    static Scene targetScene;
    static bool isMultiplayer;
    static bool startAsHost;

    public static void LoadScene(Scene target)
    {
        isMultiplayer = false;
        targetScene = target;

        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    public static void LoadMultiplayer(bool host)
    {
        isMultiplayer = true;
        startAsHost = host;

        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    public static void LoaderCallback()
    {
        if (!isMultiplayer)
        {
            SceneManager.LoadScene(targetScene.ToString());
        }
        else
        {
            if (startAsHost)
                Launcher.StartAsHost();
            else
                Launcher.StartAsClient();
        }
    }
}
