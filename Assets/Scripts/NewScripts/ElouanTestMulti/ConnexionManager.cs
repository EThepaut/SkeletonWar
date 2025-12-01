using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionManager : MonoBehaviour
{
    [Header("Netcode UI")]
    [SerializeField] private Button startHostButton;

    [Header("Session Parameters")]
    [SerializeField] private int _maxPlayers;
    [SerializeField] private TextMeshProUGUI _sessionName;
    private ISession _session;

    [Header("Client Parameters")]
    [SerializeField] private TextMeshProUGUI _profileName;
    private ConnectionState _state = ConnectionState.Disconnected;

    private NetworkManager m_NetworkManager;

    private enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
    }

    private void Start()
    {
        m_NetworkManager = GetComponent<NetworkManager>();
        m_NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
    }

    private void StartGame()
    {
        NetworkManager.Singleton.StartHost();
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (m_NetworkManager.LocalClientId == clientId)
        {
            Debug.Log($"Client {clientId} is connected and can spawn {nameof(NetworkObject)}s at");
        }
    }

    private void OnGUI()
    {

    }

    private void OnDestroy()
    {
        //
    }
}