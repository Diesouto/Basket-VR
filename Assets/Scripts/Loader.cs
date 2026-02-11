using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene
    {
        MainMenuScene,
        GameScene,
        MultiplayerScene,
        LoadingScene
    }


    static Scene targetScene;


    public static void LoadScene(Scene targetScene)
    {
        Loader.targetScene = targetScene;

        // Pantalla de carga
        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
}
