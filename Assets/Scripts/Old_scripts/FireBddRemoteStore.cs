using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using System;

public class FireBddRemoteStore : MonoBehaviour
{
    public static FireBddRemoteStore Instance { get; private set; }
    private DatabaseReference dbRef;
    private FirebaseDatabase firebaseDatabase;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // on le rend persistant
        PersistenceHelpers.MakeRootPersistent(this);
        FireBddService.EnsureInitialized(() =>
        {
            firebaseDatabase = FireBddService.Instance.Database;
            dbRef = FireBddService.Instance.RootReference;
            Debug.Log("[FireBddRemoteStore] Prêt");
        });
    }

    public void SaveRemote(string playerId, RemotePlayerData data)
    {
        if (dbRef == null)
        {
            Debug.LogError("[FireBddRemoteStore] DB non initialisé");
            return;
        }

        string json = JsonUtility.ToJson(data);
        dbRef.Child("players").Child(playerId).SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                    Debug.Log("[FireBddRemoteStore] Remote save OK");
                else
                    Debug.LogError("[FireBddRemoteStore] Remote save error: " + task.Exception?.Flatten().Message);
            });
    }

    public void LoadRemote(string playerId, Action<RemotePlayerData> onResult, Action<Exception> onError)
    {
        if (dbRef == null)
        {
            onError?.Invoke(new Exception("[FireBddRemoteStore] DB non initialisé"));
            return;
        }

        dbRef.Child("players").Child(playerId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                onError?.Invoke(task.Exception);
                return;
            }

            var snap = task.Result;
            if (snap.Exists)
            {
                try
                {
                    string json = snap.GetRawJsonValue();
                    var data = JsonUtility.FromJson<RemotePlayerData>(json);
                    onResult?.Invoke(data);
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                }
            }
            else
            {
                onResult?.Invoke(null);
            }
        });
    }
}
