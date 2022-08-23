using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages instantiations of players and other arena elements of the gameplay.
/// </summary>
public class Arena : NetworkBehaviour
{
    private Camera arenaCamera; // the main camera
    [SerializeField]
    private AudioSource audioSource; // the arena music
    private GameObject playerHost; // the host player object (available on server)
    private GameObject playerClient; // the client player object (available on server)
    private GameObject bullet; // the bullet object
    private Transform spawnPoint; // the spawn for the host
    public static Arena Instance;


    void Awake()
    {
        Instance = this;
        if (SettingsMenu.levelMusicVolume > 0f)
            audioSource.Play();
    }

    public override void OnNetworkSpawn()
    {
        // initialize fields
        arenaCamera = Camera.main;
        spawnPoint = GameObject.Find("SpawnPoint").GetComponent<Transform>();

        // start game
        StartGame();
    }

    private void Update()
    {
        // Keep audio level up to date
        audioSource.volume = SettingsMenu.levelMusicVolume;
    }

    public override void OnDestroy()
    {
        Instance = null;
    }

    /// <summary>
    /// Starts the game by spawning the players and doing initial configurations.
    /// </summary>
    public void StartGame()
    {
        HideUI();
        RotateCameraClient();
        InstantiatePlayersServer();
    }

    /// <summary>
    /// Rotates the camera for the client so that he appears on the bottom left of the screen.
    /// </summary>
    public void RotateCameraClient()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            // rotate camera by 180 on z axis
            Debug.Log("[Client] Rotating camera...");
            arenaCamera.transform.Rotate(0, 0, 180);
        }
    }

    public void HideUI()
    {
        ArenaUIManager.Instance.HideText();
    }

    /// <summary>
    /// Instantiates the players in the arena.
    /// </summary>
    public void InstantiatePlayersServer()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // spawn host
            playerHost = Instantiate(Resources.Load("Prefabs/Player"), spawnPoint.position, spawnPoint.rotation) as GameObject;
            playerHost.GetComponent<NetworkObject>().SpawnWithOwnership(PlayersManager.Instance.hostId.Value);
            Debug.Log("Host player spawned");

            // spawn client
            playerClient = Instantiate(Resources.Load("Prefabs/Player"), new Vector3(-spawnPoint.position.x, -spawnPoint.position.y, spawnPoint.position.z), Quaternion.Euler(0.0f, 0.0f, 180.0f)) as GameObject;
            playerClient.GetComponent<NetworkObject>().SpawnWithOwnership(PlayersManager.Instance.clientId.Value);
            Debug.Log("Client player spawned");

            AddPlayersToDictionaryServer();
        }
    }

    /// <summary>
    /// Adds the players to the dictionary server-side.
    /// </summary>
    public void AddPlayersToDictionaryServer()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // add players to dictionary
            PlayersManager.Instance.AddPlayerToDictionary(PlayersManager.Instance.hostId.Value, playerHost.GetComponent<Player>());
            PlayersManager.Instance.AddPlayerToDictionary(PlayersManager.Instance.clientId.Value, playerClient.GetComponent<Player>());
        }
    }

    /// <summary>
    /// Instantiates the bullet in the arena, given the player owner and the equation prompt.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="equation"></param>
    /// <returns></returns>
    public bool InstantiateBullet(Player player, string equation)
    {
        // get turret position
        Vector3 turretPosition = player.turretPivot.position;

        // parse equation
        Func<double, double> equationFunc = Bullet.CreateEquation(equation);

        if (equationFunc == null)
        {
            Debug.Log("Bullet could not be spawned");
            return false;
        }

        // spawn bullet
        bullet = Instantiate(Resources.Load("Prefabs/Bullet"), turretPosition, Quaternion.identity) as GameObject;
        bullet.GetComponent<Bullet>().CreateBullet(turretPosition, equationFunc, player.playerId);
        bullet.GetComponent<NetworkObject>().SpawnWithOwnership(player.playerId);
        Debug.Log("Bullet spawned");

        // fire bullet
        bullet.GetComponent<Bullet>().FireBullet();
        Debug.Log("Bullet fired");
        return true;
    }
}
