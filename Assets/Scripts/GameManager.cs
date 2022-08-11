using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static NetworkVariable<FixedString32Bytes> arenaName = new NetworkVariable<FixedString32Bytes>("Not selected");
    public static NetworkVariable<ulong> playerTurnId = new NetworkVariable<ulong>();
    public static NetworkVariable<ulong> victoriousPlayerId = new NetworkVariable<ulong>();
    public static NetworkVariable<GamePhase> gamePhase = new NetworkVariable<GamePhase>();
    public static NetworkVariable<bool> hasStarted = new NetworkVariable<bool>();
    public static NetworkVariable<float> timeLeft = new NetworkVariable<float>();
    public static NetworkVariable<bool> serverLoaded = new NetworkVariable<bool>(false);
    public static NetworkVariable<bool> clientLoaded = new NetworkVariable<bool>(false);
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

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnArenaLoaded;
    }

    public override void OnNetworkSpawn()
    {
        gamePhase.Value = GamePhase.LOBBY;
        gamePhase.Value = GamePhase.LOBBY;
        if (!NetworkManager.Singleton.IsServer)
        {
            serverLoaded.OnValueChanged += (bool value, bool newValue) =>
            {
                Debug.Log("Old value: " + value + " New value: " + newValue);
            };
        }
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
        Debug.Log("[Client] Arena loaded: " + scene.name);
        if (scene.name == arenaName.Value)
        {
            clientLoaded.Value = true;
            ClientLoadedServerRpc();
        }
    }

    public void OnArenaLoadedServer(Scene scene, LoadSceneMode loadMode)
    {
        Debug.Log("[Server] Arena loaded: " + scene.name);
        // check if the scene name is in the array of arenas
        if (scene.name == arenaName.Value)
        {
            serverLoaded.Value = true;
            if (clientLoaded.Value)
            {
                StartGameServer();
            }
        }
    }

    [ServerRpc]
    public void ClientLoadedServerRpc()
    {
        clientLoaded.Value = true;
        if (serverLoaded.Value)
        {
            StartGameServer();
        }
    }

    public static void LoadGame()
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

    public void StartGameServer()
    {
        Debug.Log("[Server] Starting game...");
        gamePhase.Value = GamePhase.MOVING;
        hasStarted.Value = true;
        timeLeft.Value = timePerTurn;
        playerTurnId.Value = PlayersManager.playersIds[0];
        victoriousPlayerId.Value = ulong.MaxValue;
        StartGameClientRpc();
    }

    [ClientRpc]
    public void StartGameClientRpc()
    {
        Debug.Log("[Client] Starting game...");
    }

    public static void StopGameDisconnected()
    {
        Debug.Log("[Server] Game stopped because of disconnection");
        gamePhase.Value = GamePhase.INTERRUPTED;
    }

    public static void SelectRandomArena()
    {
        arenaName.Value = ScenesNames.Arenas[UnityEngine.Random.Range(0, ScenesNames.Arenas.Length)];
        Debug.Log("[Server] Arena selected: " + arenaName.Value);
    }

    // Update is called once per frame
    void Update()
    {
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
    }


    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            SceneManager.sceneLoaded -= OnArenaLoadedServer;
        }
        else
        {
            SceneManager.sceneLoaded -= OnArenaLoadedClient;
        }
    }
}
