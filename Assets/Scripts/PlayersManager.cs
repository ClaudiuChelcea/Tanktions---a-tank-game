using Unity.Netcode;
using UnityEngine;

public class PlayersManager : NetworkBehaviour
{

    public static ulong[] playersIds = new ulong[2]; // list of players ids (max 2 players)
    public static int nPlayers = 0; // number of players

    private void Awake()
    {
        DontDestroyOnLoad(this); // don't destroy this object when loading a new scene
    }

    // Start is called before the first frame update
    void Start()
    {
        // add callbacks when a player joins or leaves the game

        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            if (NetworkManager.Singleton.IsServer)
            {
                if (nPlayers < 2)
                {
                    // accept connection
                    playersIds[nPlayers++] = id;
                    NotifyConnected(id);
                }
                else
                {
                    // refuse connection
                    NetworkManager.Singleton.DisconnectClient(id);
                    Debug.Log("[Server] Too many players");
                }
            }
        };

        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
            if (NetworkManager.Singleton.IsServer)
            {
                // disconnect client and stop the game
                --nPlayers;
                NotifyDisconnected(id);
                GameManager.Instance.StopGameDisconnectedServer();
            }
        };
    }

    // Update is called once per frame
    void Update()
    {

    }

    /**
     * Notify the server that a player has connected
     */
    public void NotifyConnected(ulong id)
    {
        Debug.Log("[Server] Player " + id + " connected");
        if (ArenaUIManager.Instance != null)
        {
            ArenaUIManager.Instance.ShowConnectedMessage(id);
        }
    }

    /**
     * Notify the server that a player has disconnected
     */
    public void NotifyDisconnected(ulong id)
    {
        Debug.Log($"Client disconnected with {id}");
        if (ArenaUIManager.Instance != null)
        {
            ArenaUIManager.Instance.ShowDisconnectedMessage(id);
        }
    }
}
