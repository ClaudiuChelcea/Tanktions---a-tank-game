using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using System.Collections;

/// <summary>
/// The multiplayer menu.
/// </summary>
public class MultiplayerMenu : NetworkBehaviour
{

    [SerializeField]
    private Button startHostButton;

    [SerializeField]
    private Button startClientButton;
    [SerializeField]
    private Button backButton;

    [SerializeField]
    private TMP_Text hostLobbyText;
    [SerializeField]
    private TMP_Text titleText;

    void Start()
    {
        hostLobbyText.enabled = false;

        // add listeners for the buttons

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

        /// BACK TO MENU
        backButton.onClick.AddListener(() =>
        {
            StopAllCoroutines();
            GameManager.Instance.BackToMenu();
            Debug.Log("Back to Main Menu");
        });
    }

    void Update()
    {
    }

    /// <summary>
    /// Waits for the client to connect to the server, then loads the arena.
    /// Should be called only on the server.
    /// </summary>
    /// <returns>IEnumerator for coroutine</returns>
    IEnumerator WaitToConnect()
    {
        startHostButton.gameObject.SetActive(false);
        startClientButton.gameObject.SetActive(false);

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

        while (PlayersManager.Instance.nPlayersConnected.Value < PlayersManager.MAX_PLAYERS)
        {
            yield return null;
        }

        // load the arena after client connected
        GameManager.Instance.LoadGame();
    }

    /// <summary>
    /// Waits for the server to load the arena, then loads the arena.
    /// Should only be called from the client.
    /// </summary>
    /// <returns>IEnumerator for coroutine</returns>
    IEnumerator WaitToLoad()
    {
        startHostButton.gameObject.SetActive(false);
        startClientButton.gameObject.SetActive(false);

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

        // loads the arena after server finished loading
        GameManager.Instance.LoadGame();
    }
}
