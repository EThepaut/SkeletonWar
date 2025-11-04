using UnityEngine;
using Unity.Netcode;

public class NetworkBootstrapper : MonoBehaviour
{
    public GameObject networkManagerPrefab;
    private static bool initialized = false;

    void Awake()
    { 
        if (!initialized)
        {
            if (NetworkManager.Singleton == null)
            {
                Instantiate(networkManagerPrefab);
            }

            DontDestroyOnLoad(NetworkManager.Singleton.gameObject);
            initialized = true;
        }
        else
        {
            if (NetworkManager.Singleton != null)
                Destroy(gameObject);
        }
    }
}
