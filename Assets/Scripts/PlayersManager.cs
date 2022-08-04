using Unity.Netcode;
using UnityEngine;

public class PlayersManager : NetworkBehaviour
{
    // Singleton
    public static PlayersManager Instance { get; private set; }
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private NetworkVariable<int> playersInGame = new NetworkVariable<int>();

    public int PlayersInGame
    {
        get => playersInGame.Value;
    }

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.Log($"Client connected with {id}");
                ++playersInGame.Value;
            }
        };

        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.Log($"Client disconnected with {id}");
                --playersInGame.Value;
            }
        };
    }

    // Update is called once per frame
    void Update()
    {

    }
}