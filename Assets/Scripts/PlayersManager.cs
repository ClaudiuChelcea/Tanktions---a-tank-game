using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages the players connected to a match.
/// </summary>
public class PlayersManager : NetworkBehaviour
{
    public static int MAX_PLAYERS = 2;
    public NetworkVariable<ulong> hostId = new NetworkVariable<ulong>();
    public NetworkVariable<ulong> clientId = new NetworkVariable<ulong>();
    public Dictionary<ulong, Player> players = new Dictionary<ulong, Player>(); // (available on server)
    public NetworkVariable<int> nPlayersConnected = new NetworkVariable<int>(0);
    public static PlayersManager Instance;

    private void Awake()
    {
        DontDestroyOnLoad(this); // don't destroy this object when loading a new scene
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // add host id server-side
            hostId.Value = NetworkManager.Singleton.LocalClientId;
            nPlayersConnected.Value = 1;
            Debug.Log("[Server] My id is " + hostId.Value);
        }

        // Add callbacks for when a player joins or leaves the game

        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            if (nPlayersConnected.Value < MAX_PLAYERS)
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    // accept connection server-side
                    clientId.Value = id;
                    Debug.Log("[Client] My id is " + clientId.Value);
                    ++nPlayersConnected.Value;
                    NotifyConnected(id);
                }
            }
            else
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    // refuse connection server-side
                    NetworkManager.Singleton.DisconnectClient(id);
                    Debug.Log("[Server] Too many players");
                }
                else
                {
                    Debug.Log("[Client] Too many players");
                }
            }
        };

        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
            if (NetworkManager.Singleton.IsServer)
            {
                // stop the game and go back to lobby
                --nPlayersConnected.Value;
                NotifyDisconnected(id);
                GameManager.Instance.StopGameServerRpc(true);
            }
            else
            {
                // go back to lobby
                NotifyDisconnected(id);
                GameManager.Instance.StopGameClientServerShutdown();
            }
        };
    }

    public override void OnDestroy()
    {
        Instance = null;
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

    /// <summary>
    /// Adds an association between the id and the player object in the dictionary.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="player"></param>
    public void AddPlayerToDictionary(ulong id, Player player)
    {
        players.Add(id, player);
    }
}
