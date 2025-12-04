using TMPro;
using Unity.Netcode;
using UnityEngine;
using SkeletonWar.Tools;

public class PseudoPlayerHUD : MonoBehaviour
{
    [SerializeField]
    private NetworkVariable<NetworkString> playerNetworkName = new NetworkVariable<NetworkString>();
    public NetworkVariable<NetworkString> PlayerNetworkName => playerNetworkName;

    private bool overlaySet = false;

    void Start()
    {
        
    }

    public void SetOverlay()
    {
        var localPlayerOverlay = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        localPlayerOverlay.text = $"{playerNetworkName.Value}";
    }

    void Update()
    {
        if (!overlaySet && !string.IsNullOrEmpty(playerNetworkName.Value))
        {
            SetOverlay();
            overlaySet = true;
        }
    }
}