using TMPro;
using UnityEngine;

public class ArenaUIManager : MonoBehaviour
{
    // Singleton
    public static ArenaUIManager Instance { get; private set; }

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

    private void Start()
    {
        playersConnection.enabled = false;
    }

    [SerializeField]
    private TMP_Text playersConnection;

    public void ShowConnectedMessage(ulong id)
    {
        playersConnection.enabled = true;
        playersConnection.text = "Player " + id + " connected";
    }

    public void ShowDisconnectedMessage(ulong id)
    {
        playersConnection.enabled = true;
        playersConnection.text = "Player " + id + " disconnected";
    }

    public void DisableConnectedMessage()
    {
        playersConnection.enabled = false;
    }
}