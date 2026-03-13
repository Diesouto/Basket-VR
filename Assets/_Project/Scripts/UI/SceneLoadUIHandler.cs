using UnityEngine;
using Unity.Netcode;

// Hides a named UI GameObject when the network scene load completes.
public class SceneLoadUIHandler : MonoBehaviour
{
    [Tooltip("Name (or exact path) of the loading UI GameObject to hide on scene load complete")] 
    public string loadingObjectName = "LoadingUI";

    GameObject loadingObject;

    void Start()
    {
        loadingObject = GameObject.Find(loadingObjectName);
        if (loadingObject == null)
        {
            Debug.LogWarning($"SceneLoadUIHandler: no GameObject named '{loadingObjectName}' found in scene.");
        }

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (loadingObject != null)
        {
            loadingObject.SetActive(false);
            Debug.Log("SceneLoadUIHandler: hid loading UI after scene load complete.");
        }
    }
}
