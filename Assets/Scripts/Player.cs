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
    public Transform turretPivot;
    public TMP_Text playerName; // HUD name
    [SerializeField]
    private float aimSpeed = 100.0f;
    public int health = 100;
    public ulong playerId;
    public static Player Instance;


    void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        // Initialize id and HUD name

        if (NetworkManager.Singleton.IsServer)
        {
            if (IsOwner)
            {
                playerId = PlayersManager.Instance.hostId.Value;
            }
            else
            {
                playerId = PlayersManager.Instance.clientId.Value;
            }
        }
        else
        {
            if (IsOwner)
            {
                playerId = PlayersManager.Instance.clientId.Value;
            }
            else
            {
                playerId = PlayersManager.Instance.hostId.Value;
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
        float xAxisInput = -Input.GetAxis("Horizontal"); // range [1, -1]
        float angleDelta = xAxisInput * aimSpeed * Time.deltaTime;
        turretPivot.Rotate(0, 0, angleDelta);
    }

    public void Damage(int damage)
    {
        if (IsClient && IsOwner)
        {
            health -= damage;
            Debug.Log("Player " + playerId + " has " + health + " health remaining.");

            if (health <= 0)
            {
                Die();
            }
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}
