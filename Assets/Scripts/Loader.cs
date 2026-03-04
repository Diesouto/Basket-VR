using UnityEngine;
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
    static string clientJoinCode;

    public static void LoadScene(Scene target)
    {
        isMultiplayer = false;
        targetScene = target;

        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    public static void LoadMultiplayer(bool host, string joinCode = "")
    {
        isMultiplayer = true;
        startAsHost = host;
        clientJoinCode = joinCode;

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
            var launcher = UnityEngine.Object.FindFirstObjectByType<Launcher>();
            if (launcher == null)
            {
                Debug.LogError("Launcher not found in scene!");
                return;
            }

            if (startAsHost)
            {
                // Start host relay
                launcher.StartAsHostRelay();
            }
            else
            {
                // Start client relay with the input join code
                if (!string.IsNullOrEmpty(clientJoinCode))
                {
                    launcher.StartAsClientRelay(clientJoinCode);
                }
                else
                {
                    Debug.LogError("Join code is empty for client!");
                }
            }
        }
    }
}