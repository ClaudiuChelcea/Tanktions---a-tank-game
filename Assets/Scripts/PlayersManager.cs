using Unity.Netcode;
using UnityEngine;

public class PlayersManager : NetworkBehaviour
{
    // list of players ids (max 2 players)
    public static ulong[] playersIds = new ulong[2];
    public static int nPlayers = 0;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void NotifyConnected(ulong id)
    {
        Debug.Log("[Server] Player " + id + " connected");
        if (ArenaUIManager.Instance != null)
        {
            ArenaUIManager.Instance.ShowConnectedMessage(id);
        }
    }

    public void NotifyDisconnected(ulong id)
    {
        Debug.Log($"Client disconnected with {id}");
        if (ArenaUIManager.Instance != null)
        {
            ArenaUIManager.Instance.ShowDisconnectedMessage(id);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            if (NetworkManager.Singleton.IsServer)
            {
                if (nPlayers < 2)
                {
                    // accept connection
                    playersIds[nPlayers++] = id;
                    NotifyConnected(id);

                    // start game if 2 players connected
                    if (nPlayers == 2)
                    {
                        GameManager.LoadGame();
                    }
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
                --nPlayers;
                NotifyDisconnected(id);
                GameManager.StopGameDisconnected();
            }
        };
    }

    // Update is called once per frame
    void Update()
    {

    }
}
