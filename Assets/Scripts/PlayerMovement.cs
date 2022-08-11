using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField]
    private float moveSpeed = 1.5f;
    [SerializeField]
    private NetworkVariable<float> xDelta = new NetworkVariable<float>();
    [SerializeField]
    private NetworkVariable<float> yDelta = new NetworkVariable<float>();

    private void UpdatePosition(float xDeltaValue, float yDeltaValue)
    {
        transform.position = new Vector3(transform.position.x + xDeltaValue, transform.position.y + yDeltaValue,
            transform.position.z);
    }

    private void UpdateServer()
    {
        UpdatePosition(xDelta.Value, yDelta.Value);
    }

    private void UpdateClient()
    {
        float xAxisInput = Input.GetAxis("Horizontal"); // range [-1, 1]
        float xDeltaNew = xAxisInput * moveSpeed * Time.deltaTime;
        UpdatePosition(xDeltaNew, 0);
        UpdateClientPositionServerRpc(xDeltaNew, 0);
    }

    [ServerRpc]
    private void UpdateClientPositionServerRpc(float xDeltaNew, float yDeltaNew)
    {
        xDelta.Value = xDeltaNew;
        yDelta.Value = yDeltaNew;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.gamePhase.Value == GameManager.GamePhase.MOVING)
        {
            if (IsServer)
            {
                UpdateServer();
            }
            if (IsClient && IsOwner)
            {
                UpdateClient();
            }
        }
    }
}
