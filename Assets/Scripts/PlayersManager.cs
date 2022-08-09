using Unity.Netcode;
using UnityEngine;

public class PlayersManager : NetworkBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    [ServerRpc]
    public void NotifyConnectedServerRpc(ulong id)
    {
        ArenaUIManager.Instance.ShowConnectedMessage(id);
    }

    [ServerRpc]
    public void NotifyDisconnectedServerRpc(ulong id)
    {
        ArenaUIManager.Instance.ShowDisconnectedMessage(id);
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
