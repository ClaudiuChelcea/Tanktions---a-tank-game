using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class MultiplayerMenu : MonoBehaviour
{
    private enum Arenas
    {
        ArenaIndustrial,
        Arena2Placeholder,
        NumberOfArenas
    }

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
                SceneManager.LoadScene("LoadingScreen");
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
                MainMenu.arenaSelected = System.Enum.GetName(typeof(Arenas), (Arenas)Random.Range(0, (float)Arenas.NumberOfArenas));
                SceneManager.LoadScene("LoadingScreen");
                // hostLobby.SetActive(true);
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
                SceneManager.LoadScene("LoadingScreen");
            }
            else
                Debug.Log("Unable to start client...");
        });
    }
}
