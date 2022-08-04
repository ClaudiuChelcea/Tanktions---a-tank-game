using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ArenaIndustrial : MonoBehaviour
{
    // Get audio
    public AudioSource audioSource;
    private GameObject player;
    [SerializeField]
    private Transform spawnPoint;
    [SerializeField]
    private TMP_Text playerConnection;

    public void ShowConnectedMessage(ulong id)
    {
        playerConnection.enabled = true;
        playerConnection.text = "Player " + id + " connected";
    }

    public void ShowDisconnectedMessage(ulong id)
    {
        playerConnection.enabled = true;
        playerConnection.text = "Player " + id + " disconnected";
    }

    public void DisableConnectedMessage()
    {
        playerConnection.enabled = false;
    }

    private void Update()
    {
        // Keep audio level up to date
        audioSource.volume = SettingsMenu.levelMusicVolume;
    }

    private void Start()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
            InstantiatePlayerServerRpc();
    }

    [ServerRpc]
    private void InstantiatePlayerServerRpc()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // instantiate player
            player = Instantiate(Resources.Load("Prefabs/Player"), spawnPoint.position, spawnPoint.rotation) as GameObject;
            player.GetComponent<NetworkObject>().Spawn();
        }
        else
            Debug.Log("Cannot instantiate player from client");
    }

    // On scene load, play music
    private void Awake()
    {
        if (SettingsMenu.levelMusicVolume > 0f)
            audioSource.Play();
    }
}
