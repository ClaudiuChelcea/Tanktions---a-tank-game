using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the game state.
/// </summary>
public class GameManager : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> arenaName = new NetworkVariable<FixedString32Bytes>("Not selected");
    public NetworkVariable<ulong> playerTurnId = new NetworkVariable<ulong>();
    public NetworkVariable<ulong> victoriousPlayerId = new NetworkVariable<ulong>();
    public NetworkVariable<GamePhase> gamePhase = new NetworkVariable<GamePhase>();
    public NetworkVariable<bool> hasStarted = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> serverLoaded = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> clientLoaded = new NetworkVariable<bool>(false);
    public NetworkVariable<float> timeLeft = new NetworkVariable<float>();
    private NetworkVariable<float> timePerMove = new NetworkVariable<float>(5.0f);
    private NetworkVariable<float> timePerAim = new NetworkVariable<float>(35.0f);
    private NetworkVariable<float> timePerShot = new NetworkVariable<float>(15.0f);
    private NetworkVariable<bool> shootingSuccessful = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> bulletStopped = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> bulletHitTarget = new NetworkVariable<bool>(false);
    private bool lostConnection = false;

    public enum GamePhase
    {
        LOBBY,
        LOADING,
        WAITING,
        MOVING,
        AIMING,
        SHOOTING,
        ENDED,
        INTERRUPTED
    }

    public static GameManager Instance;


    private void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnArenaLoaded;  // add listener for the scene load event
        DontDestroyOnLoad(this);    // make sure this object is not destroyed when loading a new scene
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnArenaLoaded;  // remove listener for the scene load event
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            gamePhase.Value = GamePhase.LOBBY;  // set initial game phase
        }
        else
        {
            gamePhase.OnValueChanged += OnGamePhaseChanged; // add client listener
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            gamePhase.OnValueChanged -= OnGamePhaseChanged; // remove client listener
        }
    }

    public override void OnDestroy()
    {
        SceneManager.LoadScene(ScenesNames.MainScreen); // return to main menu on destroy
        Instance = null;
    }

    public void OnGamePhaseChanged(GamePhase value, GamePhase newValue)
    {
        Debug.Log("New gamePhase: " + newValue);
    }

    public void SelectRandomArenaServer()
    {
        arenaName.Value = ScenesNames.SelectRandomArena();
        Debug.Log("[Server] Arena selected: " + arenaName.Value);
    }

    public void OnArenaLoaded(Scene scene, LoadSceneMode loadMode)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            OnArenaLoadedServer(scene, loadMode);
        }
        else
        {
            OnArenaLoadedClient(scene, loadMode);
        }
    }

    public void OnArenaLoadedServer(Scene scene, LoadSceneMode loadMode)
    {
        Debug.Log("[Server] Scene loaded: " + scene.name);

        // proceed if the server has loaded the arena
        if (scene.name == arenaName.Value)
        {
            serverLoaded.Value = true;
            gamePhase.Value = GamePhase.WAITING;
        }
    }

    public void OnArenaLoadedClient(Scene scene, LoadSceneMode loadMode)
    {
        Debug.Log("[Client] Scene loaded: " + scene.name);

        // proceed if the client has loaded the arena
        if (scene.name == arenaName.Value)
        {
            ClientLoadedServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ClientLoadedServerRpc()
    {
        clientLoaded.Value = true;
    }

    public void LoadGame()
    {
        Debug.Log("Host id: " + PlayersManager.Instance.hostId.Value);
        Debug.Log("Client id: " + PlayersManager.Instance.clientId.Value);

        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("[Server] Loading game...");
            gamePhase.Value = GamePhase.LOADING;
            SceneManager.LoadScene(ScenesNames.LoadingScreen);
        }
        else
        {
            Debug.Log("[Client] Loading game...");
            SceneManager.LoadScene(ScenesNames.LoadingScreen);
        }
    }

    public void StartGame()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // instantiate Arena to start the game
            GameObject arena = Instantiate(Resources.Load("Prefabs/ArenaManager"), Vector3.zero, Quaternion.identity) as GameObject;
            arena.GetComponent<NetworkObject>().Spawn();

            hasStarted.Value = true;
            gamePhase.Value = GamePhase.MOVING;
            timeLeft.Value = timePerMove.Value;
            playerTurnId.Value = PlayersManager.Instance.hostId.Value;  // the host goes first
            victoriousPlayerId.Value = ulong.MaxValue;
        }
        else
        {
            Debug.Log("Client should not issue the start game command");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // game logic
        if (hasStarted.Value)
        {
            // Server-side logic

            if (NetworkManager.Singleton.IsServer)
            {
                if (gamePhase.Value == GamePhase.MOVING)
                {
                    if (timeLeft.Value <= 0.0f)
                    {
                        gamePhase.Value = GamePhase.AIMING;
                        timeLeft.Value = timePerAim.Value;
                    }
                    else
                    {
                        timeLeft.Value -= Time.deltaTime;
                    }
                }
                else if (gamePhase.Value == GamePhase.AIMING)
                {
                    if (timeLeft.Value <= 0.0f)
                    {
                        gamePhase.Value = GamePhase.SHOOTING;
                        timeLeft.Value = timePerShot.Value;
                        shootingSuccessful.Value = true;

                        // fire bullet from current player's turret
                        Arena.Instance.InstantiateBullet(PlayersManager.Instance.players[playerTurnId.Value]);
                    }
                    else
                    {
                        timeLeft.Value -= Time.deltaTime;
                    }
                }
                else if (gamePhase.Value == GamePhase.SHOOTING)
                {
                    if (shootingSuccessful.Value)
                    {
                        if (timeLeft.Value <= 0.0f)
                        {
                            BulletMissServerRpc();  // the bullet missed if time ran out
                        }
                        else
                        {
                            timeLeft.Value -= Time.deltaTime;
                        }
                    }
                }
            }

            // UI logic (for both host and client)

            if (gamePhase.Value == GamePhase.INTERRUPTED || lostConnection)
            {
                ArenaUIManager.Instance.HideText();
                ArenaUIManager.Instance.ShowInterrupted();
            }
            else if (gamePhase.Value == GamePhase.ENDED)
            {
                ArenaUIManager.Instance.HideText();
                ArenaUIManager.Instance.ShowWinner(victoriousPlayerId.Value);
            }
            else if (gamePhase.Value == GamePhase.SHOOTING)
            {
                if (!shootingSuccessful.Value)
                {
                    ArenaUIManager.Instance.ShowBulletText("Misfire! Equation invalid!");
                }
                else
                {
                    if (bulletStopped.Value)
                    {
                        if (bulletHitTarget.Value)
                        {
                            ArenaUIManager.Instance.ShowBulletText("Hit!");
                        }
                        else
                        {
                            ArenaUIManager.Instance.ShowBulletText("Miss!");
                        }
                    }
                }
            }
            else
            {
                ArenaUIManager.Instance.HideText();
                ArenaUIManager.Instance.ShowTimeLeft(timeLeft.Value);
                ArenaUIManager.Instance.ShowGamePhase(gamePhase.Value);
                ArenaUIManager.Instance.ShowTurn(playerTurnId.Value);
                ArenaUIManager.Instance.ShowEquationInput();
                if (NetworkManager.Singleton.IsHost)
                {
                    ArenaUIManager.Instance.ShowHealth(PlayersManager.Instance.players[PlayersManager.Instance.hostId.Value]);
                }
                else
                {
                    ArenaUIManager.Instance.ShowHealth(PlayersManager.Instance.players[PlayersManager.Instance.clientId.Value]);
                }
            }
        }
        else if (gamePhase.Value == GamePhase.WAITING && ArenaUIManager.Instance != null)
        {
            // start game server-side if both players loaded
            if (serverLoaded.Value && clientLoaded.Value && NetworkManager.Singleton.IsServer)
            {
                StartGame();
            }
            else
            {
                ArenaUIManager.Instance.HideText();
                ArenaUIManager.Instance.ShowWaiting();
            }
        }
    }

    [ClientRpc]
    public void ShotRevokedClientRpc()
    {
        Debug.Log("Shooting unsuccessful, turn revoked");
        if (NetworkManager.Singleton.IsServer)
        {
            shootingSuccessful.Value = false;
            Invoke("NextTurn", 2.0f);
        }
    }

    public void NextTurn()
    {
        bulletHitTarget.Value = false;
        bulletStopped.Value = false;
        playerTurnId.Value = playerTurnId.Value == PlayersManager.Instance.hostId.Value ? PlayersManager.Instance.clientId.Value : PlayersManager.Instance.hostId.Value;
        gamePhase.Value = GamePhase.MOVING;
        timeLeft.Value = timePerMove.Value;
    }

    [ServerRpc]
    public void StopGameServerRpc(bool interrupted)
    {
        if (interrupted)
        {
            gamePhase.Value = GamePhase.INTERRUPTED;
            Debug.Log("Opponent disconnected... Going back to menu...");
            lostConnection = true;
        }
        else
        {
            gamePhase.Value = GamePhase.ENDED;
            victoriousPlayerId.Value = playerTurnId.Value;
            Debug.Log("Game ended... Player " + victoriousPlayerId.Value + " won!");
        }

        Invoke("BackToMenu", 3.0f);
    }

    public void StopGameClientServerShutdown()
    {
        Debug.Log("[Client] Server closed... Going back to menu...");
        Invoke("BackToMenu", 3.0f);
        lostConnection = true;
    }

    public void BackToMenu()
    {
        Debug.Log("Back to Main Menu");
        Destroy(gameObject);
        Destroy(PlayersManager.Instance.gameObject);
        Destroy(NetworkManager.Singleton.gameObject);
        SceneManager.LoadScene(ScenesNames.MainScreen);
    }

    public bool HasEnded()
    {
        return gamePhase.Value == GamePhase.ENDED || gamePhase.Value == GamePhase.INTERRUPTED;
    }

    [ServerRpc(RequireOwnership = false)]
    public void BulletHitPlayerServerRpc(ulong playerId, int damage)
    {
        bulletStopped.Value = true;
        bulletHitTarget.Value = true;
        Debug.Log("[Server] Player " + playerId + " hit");
        Player player = PlayersManager.Instance.players[playerId];
        player.health.Value -= damage;
        Debug.Log("[Server] Player " + playerId + " has been damaged by " + damage);

        if (player.health.Value <= 0)
        {
            player.Die();
            Debug.Log("[Server] Player " + playerId + " is dead");
            victoriousPlayerId.Value = playerTurnId.Value;
            gamePhase.Value = GamePhase.ENDED;
        }
        else
        {
            Invoke("NextTurn", 2.0f);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void BulletHitObstacleServerRpc()
    {
        bulletStopped.Value = true;
        bulletHitTarget.Value = false;
        Debug.Log("[Server] Hit an obstacle");
        Invoke("NextTurn", 2.0f);
    }

    [ServerRpc(RequireOwnership = false)]
    public void BulletMissServerRpc()
    {
        bulletStopped.Value = true;
        bulletHitTarget.Value = false;
        Debug.Log("[Server] Bullet missed");
        Invoke("NextTurn", 2.0f);
    }

    [ServerRpc(RequireOwnership = false)]
    public void BulletFireFailedServerRpc()
    {
        bulletStopped.Value = true;
        bulletHitTarget.Value = false;
        Debug.Log("[Server] Bullet fire failed");
        ShotRevokedClientRpc();
    }
}
