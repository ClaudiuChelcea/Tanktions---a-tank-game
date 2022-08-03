using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Button startServerButton;

    [SerializeField]
    private Button startHostButton;

    [SerializeField]
    private Button startClientButton;

    [SerializeField]
    private TextMeshProUGUI playersInGameText;


    private void Awake()
    {
        Cursor.visible = true;
    }

    void Update()
    {
        playersInGameText.text = $"Players in game: {PlayersManager.Instance.PlayersInGame}";
    }

    void Start()
    {
        // START SERVER
        startServerButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartServer())
                Debug.Log("Server started...");
            else
                Debug.Log("Unable to start server...");
        });

        // START HOST
        startHostButton.onClick.AddListener(async () =>
        {
            if (NetworkManager.Singleton.StartHost())
                Debug.Log("Host started...");
            else
                Debug.Log("Unable to start host...");
        });

        // START CLIENT
        startClientButton.onClick.AddListener(async () =>
        {
            if (NetworkManager.Singleton.StartClient())
                Debug.Log("Client started...");
            else
                Debug.Log("Unable to start client...");
        });

        // STATUS TYPE CALLBACKS
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            Debug.Log($"{id} just connected...");
        };
    }
}
