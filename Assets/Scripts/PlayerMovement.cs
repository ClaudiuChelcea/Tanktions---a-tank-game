using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField]
    private float moveSpeed = 1.5f;
    [SerializeField]
    private Transform turretPivot;
    [SerializeField]
    private float aimSpeed = 100.0f;
    private ulong playerId;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (IsClient && IsOwner && playerId == PlayersManager.Instance.playersIds[GameManager.Instance.playerTurnIdx.Value])
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

        if (!NetworkManager.Singleton.IsServer)
        {
            xAxisInput = -xAxisInput;
        }

        float xDelta = xAxisInput * moveSpeed * Time.deltaTime;
        transform.Translate(xDelta, 0, 0);
    }

    private void AimClient()
    {
        float xAxisInput = Input.GetAxis("Horizontal"); // range [-1, 1]

        if (!NetworkManager.Singleton.IsServer)
        {
            xAxisInput = -xAxisInput;
        }

        float angleDelta = xAxisInput * aimSpeed * Time.deltaTime;
        turretPivot.Rotate(0, 0, angleDelta);
    }
}
