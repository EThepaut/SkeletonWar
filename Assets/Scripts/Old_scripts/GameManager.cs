using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;

    void Start()
    {
        Vector3 pos = new Vector3(-23, 76, 83);
        GameObject player = Instantiate(playerPrefab, pos, Quaternion.identity);
    }

    void Update()
    {
        if (Keyboard.current.nKey.isPressed)
        {
            int sceneLvl = SceneManager.GetActiveScene().buildIndex;
            if (sceneLvl != 3)
                SceneManager.LoadScene("Stage3p", LoadSceneMode.Single);
            else
                SceneManager.LoadScene("Stage2p", LoadSceneMode.Single);
        }
    }
}