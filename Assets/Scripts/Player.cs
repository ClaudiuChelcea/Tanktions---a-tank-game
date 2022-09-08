using Unity.Netcode;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages the movement and aim mechanics for the player tank.
/// </summary>
public class Player : NetworkBehaviour
{
    [SerializeField]
    private float moveSpeed = 1.5f;
    [SerializeField]
    public Transform turretPeak;
    public TMP_Text playerName; // HUD name
    public NetworkVariable<int> health = new NetworkVariable<int>(100);
    public ulong playerId;

    public override void OnNetworkSpawn()
    {
        // Initialize id and HUD name

        if (NetworkManager.Singleton.IsServer)
        {
            if (IsOwner)
            {
                playerId = PlayersManager.Instance.hostId.Value;
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                rb.gravityScale = 1;
            }
            else
            {
                playerId = PlayersManager.Instance.clientId.Value;
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                rb.gravityScale = -1;
            }
        }
        else
        {
            if (IsOwner)
            {
                playerId = PlayersManager.Instance.clientId.Value;
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                rb.gravityScale = -1;

            }
            else
            {
                playerId = PlayersManager.Instance.hostId.Value;
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                rb.gravityScale = 1;
            }
        }

        playerName.text = "Player " + playerId;
    }

    void Update()
    {
        // only accept input if it is our turn
        if (IsClient && IsOwner && playerId == GameManager.Instance.playerTurnId.Value)
        {
            // manage movement phase
            if (GameManager.Instance.gamePhase.Value == GameManager.GamePhase.MOVING)
            {
                MoveClient();
            }

            // manage aiming phase
            if (GameManager.Instance.gamePhase.Value == GameManager.GamePhase.AIMING)
            {
                AimClient();
            }
        }
    }

    private void MoveClient()
    {
        float xAxisInput = Input.GetAxis("Horizontal"); // range [-1, 1]
        float xDelta = xAxisInput * moveSpeed * Time.deltaTime;
        transform.Translate(xDelta, 0, 0);
    }

    private void AimClient()
    {
    }

    public void Die()
    {
        GetComponent<NetworkObject>().Despawn();
    }
}
