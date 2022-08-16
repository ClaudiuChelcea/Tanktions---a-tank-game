using Unity.Netcode;
using UnityEngine;

public class Arena : NetworkBehaviour
{
    // Get audio
    public AudioSource audioSource;
    private GameObject playerHost;
    private GameObject playerClient;
    [SerializeField]
    private Transform spawnPoint;
    public static Arena Instance;


    void Awake()
    {
        Instance = this;
        if (SettingsMenu.levelMusicVolume > 0f)
            audioSource.Play();
    }

    private void Update()
    {
        // Keep audio level up to date
        audioSource.volume = SettingsMenu.levelMusicVolume;
    }

    private void Start()
    {
    }

    public void StartGame()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            InstantiatePlayer();
        }
        else
        {
            Debug.Log("Client should not be starting game");
        }
    }

    public void InstantiatePlayer()
    {
        playerHost = Instantiate(Resources.Load("Prefabs/Player"), spawnPoint.position, spawnPoint.rotation) as GameObject;
        playerHost.GetComponent<NetworkObject>().SpawnWithOwnership(PlayersManager.playersIds[0]);
        Debug.Log("Host player spawned");

        playerClient = Instantiate(Resources.Load("Prefabs/Player"), new Vector3(-spawnPoint.position.x, -spawnPoint.position.y, spawnPoint.position.z), Quaternion.Euler(0.0f, 0.0f, 180.0f)) as GameObject;
        playerClient.GetComponent<NetworkObject>().SpawnWithOwnership(PlayersManager.playersIds[1]);
        Debug.Log("Client player spawned");
    }
}
