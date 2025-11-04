using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;
    public Button serverButton;
    public Button logoutButton;

    private PlayerManager playerManager;

    void Start()
    {
        playerManager = PlayerManager.Instance;

        if (hostButton != null)
            hostButton.onClick.AddListener(OnHostClick);

        if (clientButton != null)
            clientButton.onClick.AddListener(OnClientClick);

        if (serverButton != null)
            serverButton.onClick.AddListener(OnServerClick);

        if (logoutButton != null)
            logoutButton.onClick.AddListener(OnLogoutButtonClick);
    }

    void OnHostClick()
    {
        var sceneName = $"Stage2p";

        SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);

        //if (NetworkManager.Singleton.StartHost())
        //{
        //    var sceneName = $"Stage{playerManager.data.stage}";
        //    NetworkManager.Singleton.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        //}
        //else
        //{
        //    Debug.LogError("Failed to start host.");
        //}
    }

    void OnClientClick()
    {
        if (NetworkManager.Singleton.StartClient())
        {
            var sceneName = $"Stage{playerManager.data.stage}";
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        else
            Debug.LogError("Failed to start client.");
    }

    void OnServerClick()
    {
        if (NetworkManager.Singleton.StartServer())
            Debug.Log("Server started successfully.");
        else
            Debug.LogError("Failed to start server.");
    }

    void OnLogoutButtonClick()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("ConnexionScene");
    }
}