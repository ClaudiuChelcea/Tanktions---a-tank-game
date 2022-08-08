using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static NetworkVariable<FixedString32Bytes> arenaName = new NetworkVariable<FixedString32Bytes>();
    public bool hasStarted = false;
    public float timeLeft = 180; // in seconds


    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            arenaName.Value = "Not selected";
            Debug.Log("[Server] Initial Arena: " + arenaName.Value);
        }
        else
        {
            arenaName.OnValueChanged += OnArenaChanged;
            Debug.Log("[Client] Initial Arena: " + arenaName.Value);
            if (arenaName.Value != "Not selected")
            {
                Debug.Log("[Client] Loading initial arena: " + arenaName.Value);
                // TODO: fix the loading screen bug
                SceneManager.LoadScene(ScenesNames.LoadingScreen);
            }
        }
    }

    public static void SelectRandomArena()
    {
        arenaName.Value = ScenesNames.Arenas[Random.Range(0, ScenesNames.Arenas.Length)];
        Debug.Log("[Server] Arena selected: " + arenaName.Value);
    }

    private void OnArenaChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        Debug.Log("[Client] Arena changed: " + newValue);
        Debug.Log("[Client] Loading arena: " + newValue);
        // TODO: fix the loading screen bug
        SceneManager.LoadScene(ScenesNames.LoadingScreen);
    }

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update()
    {
        if (hasStarted)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                // restart match
            }
        }
    }

    public string GetSceneName()
    {
        return arenaName.Value.ToString();
    }
}
