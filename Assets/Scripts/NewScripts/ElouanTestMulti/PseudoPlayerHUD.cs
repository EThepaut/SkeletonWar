using TMPro;
using Unity.Netcode;
using UnityEngine;
using SkeletonWar.Tools;

public class PseudoPlayerHUD : NetworkBehaviour
{
    [Header("Client Information")]
    [SerializeField] private TextMeshProUGUI _profileNameUI;
    private string _profileName;

    private NetworkVariable<NetworkString> playerNetworkName = new NetworkVariable<NetworkString>();

    private bool overlaySet = false;

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            playerNetworkName.Value = _profileName;
        }
    }

    public void ReadPseudoInput(string pseudo)
    {
        _profileName = pseudo;
        Debug.Log(_profileName);
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