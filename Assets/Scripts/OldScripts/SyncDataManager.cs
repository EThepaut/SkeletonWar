using UnityEngine;
using System;

public class SyncDataManager : MonoBehaviour
{
    public static SyncDataManager Instance { get; private set; }

    public JsonLocalStore localStore;
    public FireBddRemoteStore remoteStore;
    public PlayerManager playerManager;

    public string PlayerId { get; private set; }
    public bool IsInitialized { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        PersistenceHelpers.MakeRootPersistent(this);

        // Fallback :
        if (localStore == null) localStore = FindFirstObjectByType<JsonLocalStore>();
        if (remoteStore == null) remoteStore = FindFirstObjectByType<FireBddRemoteStore>();
        if (playerManager == null) playerManager = FindFirstObjectByType<PlayerManager>();
    }

    // Sauvegarde :
    public void SaveAll()
    {
        if (playerManager == null)
        {
            Debug.LogWarning("[SyncDataManager] SaveAll called but playerManager is null.");
            return;
        }

        var local = playerManager.ToLocalData();
        local.playerId = PlayerId;
        localStore?.SaveLocal(local);

        if (!string.IsNullOrEmpty(PlayerId))
        {
            remoteStore?.SaveRemote(PlayerId, playerManager.ToRemoteData());
        }
    }

    public void SetPlayerId(string newPlayerId)
    {
        if (string.IsNullOrEmpty(newPlayerId))
        {
            PlayerId = null;
            return;
        }
        PlayerId = newPlayerId.Trim();
    }

    public void LoadPlayerData(Action onComplete = null)
    {
        Debug.Log("[SyncDataManager] LoadPlayerData START");

        // local :
        var local = localStore?.LoadLocal();
        if (local != null && playerManager != null)
        {
            playerManager.FromLocalData(local);
            Debug.Log("[SyncDataManager] Applied local data to playerId from local: " + local.playerId);

            if (!string.IsNullOrEmpty(local.playerId) && string.IsNullOrEmpty(PlayerId))
            {
                PlayerId = local.playerId.Trim();
                Debug.Log("[SyncDataManager] PlayerId set from local file: " + PlayerId);
            }
        }

        // remote :
        if (!string.IsNullOrEmpty(PlayerId) && remoteStore != null && playerManager != null)
        {
            FireBddService.EnsureInitialized(() =>
            {
                remoteStore.LoadRemote(PlayerId, remoteData =>
                {
                    if (remoteData != null)
                    {
                        playerManager.FromRemoteData(remoteData);
                        Debug.Log("[SyncDataManager] Applied remote data for playerId: " + PlayerId);

                        if (local != null)
                        {
                            playerManager.FromLocalData(local);
                            Debug.Log("[SyncDataManager] Re-applied local prefs after remote.");
                        }
                    }
                    else
                    {
                        Debug.Log("[SyncDataManager] No remote data found for playerId: " + PlayerId);
                    }

                    IsInitialized = true;
                    onComplete?.Invoke();
                }, err =>
                {
                    Debug.LogWarning("[SyncDataManager] Load remote failed: " + err?.Message);
                    IsInitialized = true;
                    onComplete?.Invoke();
                });
            });
        }
        else
        {   
            Debug.Log("[SyncDataManager] Skipping remote load (no playerId or remoteStore null).");
            IsInitialized = true;
            onComplete?.Invoke();
        }
    }

    public static SyncDataManager EnsureInstanceFromResources()
    {
        if (Instance != null) return Instance;
        var prefab = Resources.Load<GameObject>("SyncDataManager");
        if (prefab != null) Instantiate(prefab);
        return Instance;
    }

}
