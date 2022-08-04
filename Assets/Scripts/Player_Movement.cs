using Unity.Netcode;
using UnityEngine;

public class Player_Movement : NetworkBehaviour
{
    [SerializeField]
    private float moveSpeed = 1.5f;
    [SerializeField]
    private NetworkVariable<float> xDelta = new NetworkVariable<float>();
    [SerializeField]
    private NetworkVariable<float> yDelta = new NetworkVariable<float>();

    private void UpdateServer()
    {
        transform.position = new Vector3(transform.position.x + xDelta.Value, transform.position.y + yDelta.Value,
            transform.position.z);
    }

    private void UpdateClient()
    {
        float xAxisInput = Input.GetAxis("Horizontal"); // range [-1, 1]
        UpdateClientPositionServerRpc(xAxisInput * moveSpeed * Time.deltaTime, 0);
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
