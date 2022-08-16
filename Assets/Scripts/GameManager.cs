using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> arenaName = new NetworkVariable<FixedString32Bytes>("Not selected");
    public NetworkVariable<ulong> playerTurnId = new NetworkVariable<ulong>();
    public NetworkVariable<ulong> victoriousPlayerId = new NetworkVariable<ulong>();
    public NetworkVariable<GamePhase> gamePhase = new NetworkVariable<GamePhase>();
    public NetworkVariable<bool> hasStarted = new NetworkVariable<bool>();
    public NetworkVariable<float> timeLeft = new NetworkVariable<float>();
    public NetworkVariable<bool> serverLoaded = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> clientLoaded = new NetworkVariable<bool>(false);
    public static float timePerTurn = 60.0f;

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
                if (gamePhase.Value == GamePhase.WAITING)
                {
                    ArenaUIManager.Instance.ShowWaiting();
                }
                else if (gamePhase.Value == GamePhase.MOVING || gamePhase.Value == GamePhase.AIMING)
                {
                    if (timeLeft.Value <= 0.0f)
                    {
                        // end of turn
                        timeLeft.Value = timePerTurn;
                        if (playerTurnId.Value == PlayersManager.playersIds[0])
                        {
                            playerTurnId.Value = PlayersManager.playersIds[1];
                        }
                        else
                        {
                            playerTurnId.Value = PlayersManager.playersIds[0];
                            // also change the phase
                            if (gamePhase.Value == GamePhase.MOVING)
                            {
                                gamePhase.Value = GamePhase.AIMING;
                            }
                            else
                            {
                                gamePhase.Value = GamePhase.MOVING;
                            }
                        }
                    }
                    else
                    {
                        timeLeft.Value -= Time.deltaTime;
                    }
                }
            }


            if (gamePhase.Value == GamePhase.ENDED)
            {
                ArenaUIManager.Instance.HideText();
                ArenaUIManager.Instance.ShowWinner(victoriousPlayerId.Value);
            }
            else if (gamePhase.Value == GamePhase.INTERRUPTED)
            {
                ArenaUIManager.Instance.HideText();
                ArenaUIManager.Instance.ShowInterrupted();
            }
            else
            {
                ArenaUIManager.Instance.ShowTimeLeft(timeLeft.Value);
                ArenaUIManager.Instance.ShowGamePhase(gamePhase.Value);
                ArenaUIManager.Instance.ShowTurn(playerTurnId.Value);
            }
        }

        if (gamePhase.Value == GamePhase.WAITING)
        {
            ArenaUIManager.Instance.HideText();
            ArenaUIManager.Instance.ShowWaiting();
        }
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
            Debug.Log("[Server] Starting game...");
            StartGameClientRpc();
            gamePhase.Value = GamePhase.MOVING;
            hasStarted.Value = true;
            timeLeft.Value = timePerTurn;
            playerTurnId.Value = PlayersManager.playersIds[0];
            victoriousPlayerId.Value = ulong.MaxValue;
            Arena.Instance.StartGame();
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
    }

    public void StopGameDisconnectedServer()
    {
        Debug.Log("[Server] Game stopped because of disconnection");
        gamePhase.Value = GamePhase.INTERRUPTED;
    }

    public void SelectRandomArenaServer()
    {
        arenaName.Value = ScenesNames.Arenas[UnityEngine.Random.Range(0, ScenesNames.Arenas.Length)];
        Debug.Log("[Server] Arena selected: " + arenaName.Value);
    }
}
