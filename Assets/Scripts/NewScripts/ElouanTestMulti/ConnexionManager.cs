using TMPro;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionManager : MonoBehaviour
{
    [Header("Netcode UI")]
    [SerializeField] private Button playButton; //StartClientButton

    [Header("Session Parameters")]
    [SerializeField] private int _maxPlayersBySession;

    private NetworkManager m_NetworkManager;

    private void Start()
    {
        m_NetworkManager = GetComponent<NetworkManager>();
        //if (!m_NetworkManager.IsServer && NetworkManager.Singleton.StartServer())
        //    Debug.Log("Server started...");
        //else if(m_NetworkManager.IsServer)
        //    Debug.LogWarning("Server did not started cause there is already one started");
        //else
        //    Debug.Log("Server did not started...");
    }
    public void StartGame()
    {
        if (m_NetworkManager.ConnectedClients.Count < _maxPlayersBySession)
            if (NetworkManager.Singleton.StartHost())
                Debug.Log("Client started...");
            else
                Debug.LogWarning("Client did not started...");
        else
                Debug.Log("Session full"); 
    }
}