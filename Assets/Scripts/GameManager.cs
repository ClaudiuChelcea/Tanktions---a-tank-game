using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> arenaName = new NetworkVariable<FixedString32Bytes>("Not selected");
    public NetworkVariable<int> playerTurnIdx = new NetworkVariable<int>();
    public NetworkVariable<ulong> victoriousPlayerId = new NetworkVariable<ulong>();
    public NetworkVariable<GamePhase> gamePhase = new NetworkVariable<GamePhase>();
    public NetworkVariable<bool> hasStarted = new NetworkVariable<bool>();
    public NetworkVariable<float> timeLeft = new NetworkVariable<float>();
    public NetworkVariable<bool> serverLoaded = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> clientLoaded = new NetworkVariable<bool>(false);
    private float timePerMove = 10.0f;
    private float timePerAim = 10.0f;

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
            gamePhase.Value = GamePhase.LOBBY;
        }
        else
        {
            gamePhase.OnValueChanged += OnGamePhaseChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            gamePhase.OnValueChanged -= OnGamePhaseChanged;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // game logic
        if (hasStarted.Value)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                if (gamePhase.Value == GamePhase.MOVING)
                {
                    if (timeLeft.Value <= 0.0f)
                    {
                        gamePhase.Value = GamePhase.AIMING;
                        timeLeft.Value = timePerAim;
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
                        timeLeft.Value = 0.0f;
                    }
                    else
                    {
                        timeLeft.Value -= Time.deltaTime;
                    }
                }
                else if (gamePhase.Value == GamePhase.SHOOTING)
                {
                    // trigger next game phase after bullet despawned

                    // next turn
                    playerTurnIdx.Value = (playerTurnIdx.Value + 1) % PlayersManager.Instance.nPlayers;
                    gamePhase.Value = GamePhase.MOVING;
                    timeLeft.Value = timePerMove;
                }
            }

            if (gamePhase.Value == GamePhase.INTERRUPTED)
            {
                ArenaUIManager.Instance.HideText();
                ArenaUIManager.Instance.ShowInterrupted();
            }
            else if (gamePhase.Value == GamePhase.ENDED)
            {
                ArenaUIManager.Instance.ShowWinner(victoriousPlayerId.Value);
            }
            else
            {
                ArenaUIManager.Instance.ShowTimeLeft(timeLeft.Value);
                ArenaUIManager.Instance.ShowGamePhase(gamePhase.Value);
                ArenaUIManager.Instance.ShowTurn(PlayersManager.Instance.playersIds[playerTurnIdx.Value]);
            }

        }
        else if (gamePhase.Value == GamePhase.WAITING && ArenaUIManager.Instance != null)
        {
            ArenaUIManager.Instance.ShowWaiting();
        }
    }

    public string GetArenaName()
    {
        return arenaName.Value.ToString();
    }

    public void OnGamePhaseChanged(GamePhase value, GamePhase newValue)
    {
        Debug.Log("New gamePhase: " + newValue);
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

    public void OnArenaLoadedClient(Scene scene, LoadSceneMode loadMode)
    {
        Debug.Log("[Client] Scene loaded: " + scene.name);
        // proceed if the client has loaded the arena
        if (scene.name == arenaName.Value)
        {
            ClientLoadedServerRpc();
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
            if (clientLoaded.Value)
            {
                StartGame();
            }
            else
            {
                Debug.Log("[Server] Waiting for client to load");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ClientLoadedServerRpc()
    {
        if (serverLoaded.Value)
        {
            StartGame();
        }
    }

    public void LoadGame()
    {
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
            gamePhase.Value = GamePhase.MOVING;
            StartGameClientRpc();
            ArenaUIManager.Instance.HideText();
            hasStarted.Value = true;
            timeLeft.Value = timePerMove;
            playerTurnIdx.Value = 0;
            victoriousPlayerId.Value = ulong.MaxValue;
        }
        else
        {
            Debug.Log("Client should not issue the start game command");
        }
    }

    [ClientRpc]
    public void StartGameClientRpc()
    {
        Debug.Log("[Client] Starting game...");
        Arena.Instance.StartGame();
        ArenaUIManager.Instance.HideText();
    }

    [ServerRpc]
    public void StopGameServerRpc(bool interrupted)
    {
        if (interrupted)
        {
            gamePhase.Value = GamePhase.INTERRUPTED;
            Debug.Log("Opponent disconnected... Going back to menu...");
        }
        else
        {
            gamePhase.Value = GamePhase.ENDED;
            victoriousPlayerId.Value = PlayersManager.Instance.playersIds[playerTurnIdx.Value];
            Debug.Log("Game ended... Player " + victoriousPlayerId.Value + " won!");
        }

        Invoke("BackToMenu", 3.0f);
    }

    public void BackToMenu()
    {
        Destroy(gameObject);
        Destroy(PlayersManager.Instance.gameObject);
        Destroy(NetworkManager.Singleton.gameObject);
    }

    public void StopGameClientServerShutdown()
    {
        Debug.Log("[Client] Server closed... Going back to menu...");
        ArenaUIManager.Instance.ShowInterrupted();
        Invoke("BackToMenu", 3.0f);
    }

    public bool HasEnded()
    {
        return gamePhase.Value == GamePhase.ENDED || gamePhase.Value == GamePhase.INTERRUPTED;
    }

    public void SelectRandomArenaServer()
    {
        arenaName.Value = ScenesNames.Arenas[UnityEngine.Random.Range(0, ScenesNames.Arenas.Length)];
        Debug.Log("[Server] Arena selected: " + arenaName.Value);
    }

    public override void OnDestroy()
    {
        SceneManager.LoadScene(ScenesNames.MainScreen);
        Instance = null;
    }
}
