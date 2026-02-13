using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NetworkManager))]
[RequireComponent(typeof(UnityTransport))]
public class Launcher : MonoBehaviour
{
    void Start()
    {
        // Escuchamos cuando un cliente se conecta
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        // Escuchamos cuando el servidor inicia
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
    }

    public static void StartAsClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public static void StartAsHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    void OnClientConnected(ulong clientID)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        Debug.Log("Cliente conectado: " + clientID);
    }

    void OnServerStarted()
    {
        Debug.Log("Servidor iniciado correctamente.");

        NetworkManager.Singleton.SceneManager.LoadScene(
            Loader.Scene.MultiplayerScene.ToString(),
            LoadSceneMode.Single
        );
    }
}
