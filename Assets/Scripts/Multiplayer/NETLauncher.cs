using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

using System;

[RequireComponent(typeof(NetworkManager))]
[RequireComponent(typeof(UnityTransport))]
public class Launcher : MonoBehaviour
{
    public static bool isMultiplayer = false;

    void Start()
    {
        // Escuchamos cuando un cliente se conecta
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        // Escuchamos cuando el servidor inicia
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;

        // Parse optional command-line args for address/port
        ParseCommandLineArgs();
    }

    void OnDestroy()
    {
        CleanupNetwork();
    }

    void OnApplicationQuit()
    {
        CleanupNetwork();
    }

    public static void StartAsClient()
    {
        isMultiplayer = true;
        NetworkManager.Singleton.StartClient();
    }

    public static void StartAsHost()
    {
        isMultiplayer = true;
        NetworkManager.Singleton.StartHost();
    }

    public static void StartAsClient(string address, ushort port)
    {
        isMultiplayer = true;
        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.SetConnectionData(address, port);
        NetworkManager.Singleton.StartClient();
    }

    public static void StartAsHost(string address, ushort port)
    {
        isMultiplayer = true;
        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.SetConnectionData(address, port);
        NetworkManager.Singleton.StartHost();
    }

    public static void ShutdownNetwork()
    {
        if (NetworkManager.Singleton == null) return;

        if (NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        isMultiplayer = false;
    }

    void ParseCommandLineArgs()
    {
        string[] args = Environment.GetCommandLineArgs();
        string address = null;
        ushort port = 0;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-port" && i + 1 < args.Length)
            {
                if (ushort.TryParse(args[i + 1], out ushort p)) port = p;
            }
            else if (args[i] == "-address" && i + 1 < args.Length)
            {
                address = args[i + 1];
            }
        }

        if (port != 0 || !string.IsNullOrEmpty(address))
        {
            var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (!string.IsNullOrEmpty(address))
                utp.SetConnectionData(address, port == 0 ? (ushort)7777 : port);
            else
                utp.SetConnectionData("127.0.0.1", port == 0 ? (ushort)7777 : port);
        }
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
            Loader.Scene.MultiplayerSceneVR.ToString(),
            LoadSceneMode.Single
        );
    }

    void CleanupNetwork()
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;

        if (NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
