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

        DontDestroyOnLoad(this.gameObject);
    }

    [ServerRpc]
    public void NotifyConnectedServerRpc(ulong id)
    {
        ArenaIndustrial arena = GameObject.Find("GameManager").GetComponent<ArenaIndustrial>();
        // Show message
        arena.ShowConnectedMessage(id);
    }

    [ServerRpc]
    public void NotifyDisconnectedServerRpc(ulong id)
    {
        ArenaIndustrial arena = GameObject.Find("GameManager").GetComponent<ArenaIndustrial>();
        // Show message
        arena.ShowDisconnectedMessage(id);
    }

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.Log($"Client connected with {id}");
                NotifyConnectedServerRpc(id);
            }
        };

        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.Log($"Client disconnected with {id}");
                NotifyDisconnectedServerRpc(id);
            }
        };
    }

    // Update is called once per frame
    void Update()
    {

    }
}
