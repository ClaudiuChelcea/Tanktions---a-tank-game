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

    void Update()
    {
    }

    void Start()
    {
        hostLobbyText.enabled = false;

        // START SERVER
        startServerButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartServer())
            {
                Debug.Log("Server started...");
                // select random arena
                GameManager.SelectRandomArena();
                // wait for the game to start
                StartCoroutine(WaitToLoad());
            }
            else
                Debug.Log("Unable to start server...");
        });

        // START HOST
        startHostButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Host started...");
                // select random arena
                GameManager.SelectRandomArena();
                // wait for the game to start
                StartCoroutine(WaitToLoad());
            }
            else
                Debug.Log("Unable to start host...");
        });

        // START CLIENT
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

    IEnumerator WaitToLoad()
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
            hostLobbyText.text = "Waiting for server to load...";
        }

        while (!GameManager.serverLoaded.Value)
        {
            // wait for the game to load
            yield return null;
        }

        hostLobbyText.enabled = false;

        if (!NetworkManager.Singleton.IsServer)
        {
            GameManager.LoadGame();
        }
    }
}
