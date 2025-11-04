using UnityEngine;
using UnityEngine.SceneManagement;

public class LoaderSceneManager : MonoBehaviour
{
    public string firstSceneToLoad = "ConnexionScene";

    void Start()
    {
        FireBddService.EnsureInitialized(() =>
        {
            Debug.Log("[LoaderSceneManager] Firebase initialisé. Chargement: " + firstSceneToLoad);
            SceneManager.LoadScene(firstSceneToLoad);
        });
    }
}