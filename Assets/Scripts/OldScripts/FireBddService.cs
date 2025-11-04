using System;
using System.Collections;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class FireBddService : MonoBehaviour
{
    public static FireBddService Instance { get; private set; }

    [Header("Firebase settings")]
    [SerializeField]
    private string databaseUrl = "https://coursfirebaseint2-default-rtdb.europe-west1.firebasedatabase.app";

    [SerializeField, Tooltip("Timeout pour l'attente d'initialisation lors d'EnsureInitialized")]
    private float initTimeout = 10f;

    public FirebaseAuth Auth { get; private set; }
    public FirebaseDatabase Database { get; private set; }
    public DatabaseReference RootReference { get; private set; }

    public bool IsInitialized { get; private set; } = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Initialize();
    }

    private void Initialize()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var status = task.Result;
            if (status == DependencyStatus.Available)
            {
                var app = FirebaseApp.DefaultInstance;
                Auth = FirebaseAuth.DefaultInstance;
                Database = FirebaseDatabase.GetInstance(app, databaseUrl);
                RootReference = Database.RootReference;

                IsInitialized = true;
                Debug.Log("[FireBddService] Firebase initialisé avec succès");
            }
            else
            {
                IsInitialized = false;
                Debug.LogError($"[FireBddService] Impossible de résoudre les dépendances Firebase: {status}");
            }
        });
    }

    public static void EnsureInitialized(Action callback)
    {
        // Si pas d'instance, on en crée une automatiquement 
        if (Instance == null)
        {
            Debug.LogWarning("[FireBddService] Instance manquante : création automatique d'un GameObject FireBddService");
            var go = new GameObject("FireBddService");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<FireBddService>();
        }

        if (Instance.IsInitialized)
        {
            callback?.Invoke();
            return;
        }

        Instance.StartCoroutine(Instance.WaitForInitThen(callback));
    }

    private IEnumerator WaitForInitThen(Action callback)
    {
        float t = 0f;
        while (!IsInitialized && t < initTimeout)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (!IsInitialized)
        {
            Debug.LogWarning($"[FireBddService] Init non terminée après {initTimeout}s. Le callback sera tout de même appelé.");
        }

        callback?.Invoke();
    }

    public static string CurrentUserId()
    {
        return Instance?.Auth?.CurrentUser?.UserId;
    }
}
