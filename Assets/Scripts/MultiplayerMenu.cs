using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using TMPro;
using System.Collections;

public class MultiplayerMenu : NetworkBehaviour
{
    [SerializeField]
    private Button startServerButton;

    [SerializeField]
    private Button startHostButton;

    [SerializeField]
    private Button startClientButton;

    [SerializeField]
    private TMP_Text hostLobbyText;
    [SerializeField]
    private TMP_Text titleText;

    private void Awake()
    {
        Cursor.visible = true;
    }


    void Start()
    {
        hostLobbyText.enabled = false;

        // add listeners for the buttons

        ///  SERVER
        startServerButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartServer())
            {
                Debug.Log("Server started...");
                // select random arena
                GameManager.Instance.SelectRandomArenaServer();
                // wait for the game to start
                StartCoroutine(WaitToConnect());
            }
            else
                Debug.Log("Unable to start server...");
        });

        ///  HOST
        startHostButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Host started...");
                // select random arena
                GameManager.Instance.SelectRandomArenaServer();
                // wait for the game to start
                StartCoroutine(WaitToConnect());
            }
            else
                Debug.Log("Unable to start host...");
        });

        ///  CLIENT
        startClientButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started...");
                // wait for the game to start
                StartCoroutine(WaitToLoad());
            }
            else
                Debug.Log("Unable to start client...");
        });
    }

    void Update()
    {
    }

    /// <summary>
    /// Waits for the client to connect to the server.
    /// Should be called only on the server.
    /// </summary>
    /// <returns>IEnumerator for coroutine</returns>
    IEnumerator WaitToConnect()
    {
        startHostButton.gameObject.SetActive(false);
        startClientButton.gameObject.SetActive(false);
        startServerButton.gameObject.SetActive(false);

        titleText.enabled = false;
        hostLobbyText.enabled = true;

        if (NetworkManager.Singleton.IsServer)
        {
            hostLobbyText.text = "Waiting for player to join...";
        }
        else
        {
            Debug.Log("How did we get here?");
        }

        while (PlayersManager.nPlayers < 2)
        {
            yield return null;
        }

        GameManager.Instance.LoadGame();
    }

    /// <summary>
    /// Waits for the server to load the arena.
    /// Should only be called from the client.
    /// </summary>
    /// <returns>IEnumerator for coroutine</returns>
    IEnumerator WaitToLoad()
    {
        startHostButton.gameObject.SetActive(false);
        startClientButton.gameObject.SetActive(false);
        startServerButton.gameObject.SetActive(false);

        titleText.enabled = false;
        hostLobbyText.enabled = true;

        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("How did we end up here?");
        }
        else
        {
            hostLobbyText.text = "Waiting for server to load...";
        }

        while (!GameManager.Instance.serverLoaded.Value)
        {
            // wait for the game to load
            yield return null;
        }

        hostLobbyText.enabled = false;

        GameManager.Instance.LoadGame();
    }
}
