using Unity.Netcode;
using UnityEngine;
using TMPro;

public class PlayerHUD : NetworkBehaviour
{
    private NetworkVariable<NetworkString> playerName = new NetworkVariable<NetworkString>();
    private bool overlaySet = false;
    [SerializeField]
    private TMP_Text overlay;
    [SerializeField]
    private Transform player;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            playerName.Value = $"Player {OwnerClientId}";
        }
    }

    public void SetOverlay()
    {
        overlay.text = playerName.Value;
    }

    // Start is called before the first frame update
    void Start()
    {
        SetOverlay();
    }

    // Update is called once per frame
    void Update()
    {
        if (!overlaySet && !string.IsNullOrEmpty(playerName.Value))
        {
            SetOverlay();
            overlaySet = true;
        }
    }
}
