using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using Unity.Netcode;

public class LoginManager : MonoBehaviour
{
    [Header("UI")]
    public InputField loginInput;
    public InputField passwordInput;
    public InputField pseudoInput;
    public TMP_Text messageText;
    public Button loginButton;
    public Button registerButton;

    [Header("Next scene")]
    public string sceneOnLogin = "MainMenuScene";

    private FirebaseAuth auth;
    private FirebaseDatabase database;
    private DatabaseReference dbRoot;

    void Awake()
    {
        FireBddService.EnsureInitialized(() =>
        {
            auth = FireBddService.Instance.Auth;
            database = FireBddService.Instance.Database;
            dbRoot = FireBddService.Instance.RootReference;
            Debug.Log("[LoginManager] Firebase prêt");
        });
    }

    void Start()
    {
        if (loginButton != null) loginButton.onClick.AddListener(OnLoginButtonClick);
        if (registerButton != null) registerButton.onClick.AddListener(OnRegisterButtonClick);
    }

    void OnLoginButtonClick()
    {
        if (loginInput == null || passwordInput == null)
        {
            ShowMessage("Email et mot de passe requis.");
            Debug.LogError("[LoginManager] loginInput or passwordInput is null in LoginManager inspector.");
            return;
        }

        var email = loginInput?.text?.Trim();
        var pass = passwordInput?.text;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            ShowMessage("Email et mot de passe requis.");
            return;
        }

        ShowMessage("Connexion en cours...");
        auth.SignInWithEmailAndPasswordAsync(email, pass).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                var msg = task.Exception?.Flatten().InnerException?.Message ?? task.Exception?.Message;
                ShowMessage("Email ou mot de passe incorrect !");
                Debug.LogError("[LoginManager] SignIn error: " + task.Exception);
                return;
            }

            // IMPORTANT: task.Result est un AuthResult, pas un FirebaseUser
            Firebase.Auth.AuthResult authResult = task.Result;
            FirebaseUser newUser = authResult.User;

            ShowMessage($"[LoginManager] Connecté en tant que {newUser.Email}");
            string uid = newUser.UserId;
            Debug.Log("[LoginManager] Signed in UID: " + uid);

            SyncDataManager.EnsureInstanceFromResources();
            if (SyncDataManager.Instance == null)
            {
                Debug.LogWarning("[LoginManager] SyncDataManager.Instance is null at login time. Attempting to continue anyway.");
                // fallback :
                EnsureRemoteDataExists(uid, () => SceneManager.LoadScene(sceneOnLogin));
            }
            else
            {
                SyncDataManager.Instance.SetPlayerId(uid);

                var local = SyncDataManager.Instance.playerManager?.ToLocalData() ?? new LocalPlayerData();
                local.playerId = uid;
                SyncDataManager.Instance.localStore?.SaveLocal(local);

                SyncDataManager.Instance.LoadPlayerData(() =>
                {
                    Debug.Log("[LoginManager] Sync load player data complete. Loading scene: " + sceneOnLogin);
                    SceneManager.LoadScene(sceneOnLogin);
                });
            }
        });
    }

    void OnRegisterButtonClick()
    {
        if (loginInput == null || passwordInput == null || pseudoInput == null)
        {
            ShowMessage("Email, mot de passe et pseudo requis.");
            Debug.LogError("[LoginManager] loginInput or passwordInput is null in LoginManager inspector.");
            return;
        }

        var email = loginInput?.text?.Trim();
        var pass = passwordInput?.text;
        var pseudo = pseudoInput?.text?.Trim();
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(pseudo))
        {
            ShowMessage("Email, mot de passe et pseudo requis.");
            return;
        }

        ShowMessage("Création du compte...");
        // REGISTER (Create user)
        auth.CreateUserWithEmailAndPasswordAsync(email, pass).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                var msg = task.Exception?.Flatten().InnerException?.Message ?? task.Exception?.Message;
                ShowMessage("Erreur inscription : " + msg);
                Debug.LogError("[LoginManager] CreateUser error: " + task.Exception);
                return;
            }

            Firebase.Auth.AuthResult authResult = task.Result;
            FirebaseUser newUser = authResult.User;

            ShowMessage($"Compte créé : {newUser.Email}");
            string uid = newUser.UserId;
            Debug.Log("[LoginManager] Registered UID: " + uid);

            var initialProfile = CreateDefaultRemoteData(newUser.Email, pseudo);
            string json = JsonUtility.ToJson(initialProfile);

            dbRoot.Child("players").Child(uid).SetRawJsonValueAsync(json).ContinueWithOnMainThread(dbTask =>
            {
                if (dbTask.IsCanceled || dbTask.IsFaulted)
                {
                    Debug.LogError("[LoginManager] Erreur création profil initial: " + dbTask.Exception);
                    ShowMessage("Échec création profil distant : " + dbTask.Exception?.Message);
                }
                else
                {
                    SyncDataManager.EnsureInstanceFromResources();

                    if (SyncDataManager.Instance != null)
                    {
                        SyncDataManager.Instance.SetPlayerId(uid);

                        var local = SyncDataManager.Instance.playerManager?.ToLocalData() ?? new LocalPlayerData();
                        local.playerId = uid;
                        SyncDataManager.Instance.localStore?.SaveLocal(local);

                        SyncDataManager.Instance.LoadPlayerData(() =>
                        {
                            SceneManager.LoadScene(sceneOnLogin);
                        });
                    }
                    else
                    {
                        SceneManager.LoadScene(sceneOnLogin);
                    }
                }
            });
        });
    }

    // On créer un player par defaut si il n'existe pas en bdd
    private void EnsureRemoteDataExists(string uid, Action onDone)
    {
        dbRoot.Child("players").Child(uid).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogWarning("[LoginManager] Get profile failed: " + task.Exception);
                onDone?.Invoke();
                return;
            }

            DataSnapshot snap = task.Result;
            if (!snap.Exists)
            {
                var profile = CreateDefaultRemoteData(auth.CurrentUser?.Email, "");
                dbRoot.Child("players").Child(uid).SetRawJsonValueAsync(JsonUtility.ToJson(profile)).ContinueWithOnMainThread(setTask =>
                {
                    if (setTask.IsFaulted)
                        Debug.LogWarning("[LoginManager] Create remote profile failed: " + setTask.Exception);
                    onDone?.Invoke();
                });
            }
            else
            {
                // Profil existant, on peut éventuellement merger/charger ici
                onDone?.Invoke();
            }
        });
    }

    private RemotePlayerData CreateDefaultRemoteData(string email, string pseudo)
    {
        return new RemotePlayerData
        {
            login = email ?? "",
            pseudo = pseudo ?? "",
            hpMax = 100,
            hp = 100,
            damage = 10,
            defense = 10,
            speed = 10,
            stage = 1,
            level = 1,
            xp = 0
        };
    }

    private void ShowMessage(string m)
    {
        if (messageText != null) messageText.text = m;
        Debug.Log("[LoginManager] " + m);
    }
}
