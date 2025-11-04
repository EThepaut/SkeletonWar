using UnityEngine;

public static class PersistenceHelpers
{
    // On l'appelle dans le Awake des scripts persistants pour appliquer le DontDestroyOnLoad
    // Ca permet d'avoir des scripts qui ont toujours la même Instance, peut importe la scene
    public static void MakeRootPersistent(MonoBehaviour mono)
    {
        if (mono == null) return;
        GameObject root = mono.transform.root.gameObject;
        UnityEngine.Object.DontDestroyOnLoad(root);
    }
}
