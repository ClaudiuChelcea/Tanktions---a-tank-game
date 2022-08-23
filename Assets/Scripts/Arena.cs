using Unity.Netcode;
using UnityEngine;

public class Arena : NetworkBehaviour
{
    [SerializeField]
    private Camera arenaCamera;
    [SerializeField]
    private AudioSource audioSource;
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
            InstantiatePlayers();
        }
        else
        {
            RotateCameraClient();
        }
    }

    public void RotateCameraClient()
    {
        // rotate camera by 180 on z axis
        Debug.Log("[Client] Rotating camera...");
        arenaCamera.transform.Rotate(0, 0, 180);
    }

    public void InstantiatePlayers()
    {
        playerHost = Instantiate(Resources.Load("Prefabs/Player"), spawnPoint.position, spawnPoint.rotation) as GameObject;
        playerHost.GetComponent<NetworkObject>().SpawnWithOwnership(PlayersManager.Instance.playersIds[0]);
        Debug.Log("Host player spawned");

        playerClient = Instantiate(Resources.Load("Prefabs/Player"), new Vector3(-spawnPoint.position.x, -spawnPoint.position.y, spawnPoint.position.z), Quaternion.Euler(0.0f, 0.0f, 180.0f)) as GameObject;
        playerClient.GetComponent<NetworkObject>().SpawnWithOwnership(PlayersManager.Instance.playersIds[1]);
        Debug.Log("Client player spawned");
    }

    public override void OnDestroy()
    {
        Instance = null;
    }
}
