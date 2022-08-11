using Unity.Netcode;
using UnityEngine;

public class Arena : NetworkBehaviour
{
    // Get audio
    public AudioSource audioSource;
    private GameObject player;
    [SerializeField]
    private Transform spawnPoint;


    private void Update()
    {
        // Keep audio level up to date
        audioSource.volume = SettingsMenu.levelMusicVolume;
    }

    private void Start()
    {
        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            InstantiatePlayerServerRpc();
        else
            InstantiatePlayer();
    }

    private void InstantiatePlayer()
    {
        if (NetworkManager.Singleton.IsHost)
            player = Instantiate(Resources.Load("Prefabs/Player"), spawnPoint.position, spawnPoint.rotation) as GameObject;
        else
            player = Instantiate(Resources.Load("Prefabs/Player"), new Vector3(-spawnPoint.position.x, -spawnPoint.position.y, spawnPoint.position.z), spawnPoint.rotation) as GameObject;
        player.GetComponent<NetworkObject>().Spawn();
        Debug.Log("Player spawned");
    }

    [ServerRpc(RequireOwnership = false)]
    private void InstantiatePlayerServerRpc()
    {
        InstantiatePlayer();
    }

    // On scene load, play music
    private void Awake()
    {
        if (SettingsMenu.levelMusicVolume > 0f)
            audioSource.Play();
    }
}
