using UnityEngine;
using System.IO;
using System;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

public class JsonLocalStore : MonoBehaviour, ILocalStore
{
    public static JsonLocalStore Instance { get; private set; }

    public PlayerManager playerManager;

    string path;

    [Serializable]
    private class LocalStoreFile
    {
        public LocalPlayerData[] players;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        PersistenceHelpers.MakeRootPersistent(this); // On le rend persistant

        path = Path.Combine(Application.persistentDataPath, "localData.json");

        // Fallback :
        if (playerManager == null) playerManager = FindFirstObjectByType<PlayerManager>();
    }

    public void SaveLocal(LocalPlayerData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[JsonLocalStore] SaveLocal called with null data.");
            return;
        }

        LocalStoreFile store = LoadStoreFileOrNew();

        var list = new List<LocalPlayerData>(store.players ?? new LocalPlayerData[0]);

        int idx = -1;
        if (!string.IsNullOrEmpty(data.playerId))
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (!string.IsNullOrEmpty(list[i].playerId) && list[i].playerId == data.playerId)
                {
                    idx = i;
                    break;
                }
            }
        }

        if (idx >= 0) list[idx] = data;
        else list.Add(data);

        store.players = list.ToArray();
        WriteStoreFile(store);
        Debug.Log("[JsonLocalStore] Saved local data for playerId: " + data.playerId);
        // Pour ouvrir le fichier directement :
//#if UNITY_EDITOR
//        UnityEditor.EditorUtility.RevealInFinder(path);
//#endif
    }

    public LocalPlayerData LoadLocal()
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning("[JsonLocalStore] Save file not found: " + path);
            return null;
        }

        var store = LoadStoreFileOrNew();

        if (store.players == null || store.players.Length == 0)
        {
            Debug.LogWarning("[JsonLocalStore] Aucun joueur local trouvé dans le fichier.");
            return null;
        }

        string desiredId = SyncDataManager.Instance?.PlayerId;
        if (!string.IsNullOrEmpty(desiredId))
        {
            foreach (var p in store.players)
            {
                if (!string.IsNullOrEmpty(p.playerId) && p.playerId == desiredId)
                {
                    Debug.Log("[JsonLocalStore] Loaded local data for playerId: " + desiredId);
                    return p;
                }
            }
            Debug.Log("[JsonLocalStore] Aucun local data pour playerId: " + desiredId);
            return null;
        }

        // Fallback : return first player if no PlayerId specified
        Debug.Log("[JsonLocalStore] No PlayerId specified — returning first local player.");
        return store.players[0];
    }

    private LocalStoreFile LoadStoreFileOrNew()
    {
        if (!File.Exists(path)) return new LocalStoreFile { players = new LocalPlayerData[0] };

        try
        {
            string json = File.ReadAllText(path);

            var wrapped = JsonUtility.FromJson<LocalStoreFile>(json);
            if (wrapped != null && wrapped.players != null) return wrapped;

            if (json.Contains("\"playerId\""))
            {
                var single = JsonUtility.FromJson<LocalPlayerData>(json);
                if (single != null)
                {
                    return new LocalStoreFile { players = new LocalPlayerData[] { single } };
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[JsonLocalStore] Lecture localData.json échouée: " + ex.Message);
        }

        return new LocalStoreFile { players = new LocalPlayerData[0] };
    }

    private void WriteStoreFile(LocalStoreFile store)
    {
        try
        {
            string json = JsonUtility.ToJson(store, true);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Debug.LogError("[JsonLocalStore] Écriture localData.json échouée: " + ex.Message);
        }
    }
}
