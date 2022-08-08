using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class MultiplayerMenu : NetworkBehaviour
{
    [SerializeField]
    private Button startServerButton;

    [SerializeField]
    private Button startHostButton;

    [SerializeField]
    private Button startClientButton;

    [SerializeField]
    private GameObject hostLobby;

    private void Awake()
    {
        Cursor.visible = true;
    }

    void Update()
    {
    }

    void Start()
    {
        // START SERVER
        startServerButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartServer())
            {
                Debug.Log("Server started...");
                // select random arena
                GameManager.SelectRandomArena();
                SceneManager.LoadScene(ScenesNames.LoadingScreen);
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
                SceneManager.LoadScene(ScenesNames.LoadingScreen);
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
                // arena will be retrieved from the server when the network
                // variable is synced, so we don't need to do anything here.
            }
            else
                Debug.Log("Unable to start client...");
        });
    }
}
