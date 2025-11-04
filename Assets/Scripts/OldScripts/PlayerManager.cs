using UnityEngine;
using System;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [Header("Player data")]
    public PlayerData data;

    public event Action<PlayerData> OnPlayerDataChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        PersistenceHelpers.MakeRootPersistent(this);
    }

    void Start()
    {
        if (data == null) data = new PlayerData();
        if (data.hp <= 0) data.hp = data.hpMax;
        OnPlayerDataChanged?.Invoke(data);
    }

    public LocalPlayerData ToLocalData()
    {
        return new LocalPlayerData
        {
            playerId = null,
            weaponIndex = data.weaponIndex,
            volumeSound = data.volumeSound
        };
    }

    public void FromLocalData(LocalPlayerData s)
    {
        if (s == null) return;
        data.weaponIndex = s.weaponIndex;
        data.volumeSound = s.volumeSound;
        OnPlayerDataChanged?.Invoke(data);
    }

    public RemotePlayerData ToRemoteData()
    {
        return new RemotePlayerData
        {
            login = data.login,
            pseudo = data.pseudo,
            hpMax = data.hpMax,
            hp = data.hp,
            damage = data.damage,
            defense = data.defense,
            speed = data.speed,
            stage = data.stage,
            level = data.level,
            xp = data.xp
        };
    }

    public void FromRemoteData(RemotePlayerData p)
    {
        if (p == null) return;

        data.login = p.login;
        data.pseudo = p.pseudo;
        data.hpMax = p.hpMax;
        data.hp = p.hp;
        data.damage = p.damage;
        data.defense = p.defense;
        data.speed = p.speed;
        data.stage = p.stage;
        data.level = p.level;
        data.xp = p.xp;
        OnPlayerDataChanged?.Invoke(data);
    }

    public PlayerData ToData()
    {
        return data;
    }

    public void FromData(PlayerData d)
    {
        if (d == null) return;

        data = d;
        OnPlayerDataChanged?.Invoke(data);
    }

    public void SetPseudo(string pseudo)
    {
        if (string.IsNullOrEmpty(pseudo)) return;
        data.pseudo = pseudo;
        OnPlayerDataChanged?.Invoke(data);
    }

    public void SetLogin(string login)
    {
        data.login = login;
        OnPlayerDataChanged?.Invoke(data);
    }

    public void SetWeaponByIndex(int index)
    {
        switch (index)
        {
            case 0: 
                data.weaponIndex = 0;
                break;
            case 1: 
                data.weaponIndex = 1;
                break;
            case 2: 
                data.weaponIndex = 2;
                break;
            default:
                data.weaponIndex = -1;
                break;
        }
        OnPlayerDataChanged?.Invoke(data);
    }

    public void ChangeHp(int value)
    {
        data.hp = Mathf.Clamp(data.hp + value, 0, data.hpMax);
        OnPlayerDataChanged?.Invoke(data);
    }

    public void ChangeHpMax(int value)
    {
        data.hp += value;
        data.hpMax += value;
        OnPlayerDataChanged?.Invoke(data);
    }

    public void ChangeDamage(int value)
    {
        data.damage = Mathf.Max(0, data.damage + value);
        OnPlayerDataChanged?.Invoke(data);
    }

    public void ChangeDefense(int value)
    {
        data.defense = Mathf.Max(0, data.defense + value);
        OnPlayerDataChanged?.Invoke(data);
    }

    public void ChangeSpeed(int value)
    {
        data.speed = Mathf.Max(0, data.speed + value);
        OnPlayerDataChanged?.Invoke(data);
    }

    public void ChangeVolumeSound(bool value)
    {
        data.volumeSound = value;
        OnPlayerDataChanged?.Invoke(data);
    }

    public void ToggleVolumeSound()
    {
        ChangeVolumeSound(!data.volumeSound);
    }

    public bool ApproximatelyColor(Color a, Color b, float tolerance = 0.01f)
    {
        return Mathf.Abs(a.r - b.r) <= tolerance &&
               Mathf.Abs(a.g - b.g) <= tolerance &&
               Mathf.Abs(a.b - b.b) <= tolerance &&
               Mathf.Abs(a.a - b.a) <= tolerance;
    }

    public void GainLevel()
    {
        data.level++;
        GainStats();
    }

    public void GainXP()
    {
        int xpGoal = (int)(((data.level * 100) * 1.5) + data.level * 100);

        data.xp += 50;

        if(data.xp == xpGoal)
            GainLevel();
    }

    public void GainStats()
    {
        data.hpMax = data.hpMax + 3;
        data.hp = data.hp + 3;
        data.damage = data.damage + 2;
        data.defense = data.defense + 4;
    }

    // ---------------------------------------------PASSWORD ------------------------------------------------------
    // UpdateEmail Non fonctionnelle car SendEmailVerificationBeforeUpdatingEmailAsync ne fonctionne pas,  
    public void UpdateEmail(string newEmail, string currentPassword, Action onSuccess, Action<Exception> onError)
    {
        var auth = FirebaseAuth.DefaultInstance;
        var user = auth.CurrentUser;
        if (user == null)
        {
            onError?.Invoke(new Exception("Aucun utilisateur connect�."));
            return;
        }
        if (string.IsNullOrEmpty(newEmail))
        {
            onError?.Invoke(new Exception("Email vide."));
            return;
        }
        if (string.IsNullOrEmpty(currentPassword))
        {
            onError?.Invoke(new Exception("Mot de passe actuel requis."));
            return;
        }

        var credential = EmailAuthProvider.GetCredential(user.Email, currentPassword);

        user.ReauthenticateAsync(credential).ContinueWithOnMainThread(reauthTask =>
        {
            if (reauthTask.IsFaulted || reauthTask.IsCanceled)
            {
                onError?.Invoke(reauthTask.Exception ?? new Exception("R�-authentification �chou�e."));
                return;
            }

            user.SendEmailVerificationBeforeUpdatingEmailAsync(newEmail).ContinueWithOnMainThread(updateTask =>
            {
                if (updateTask.IsFaulted || updateTask.IsCanceled)
                {
                    onError?.Invoke(updateTask.Exception ?? new Exception("�chec mise � jour email."));
                    return;
                }

                SetLogin(newEmail);

                try
                {
                    var db = FirebaseDatabase.DefaultInstance;
                    db.RootReference.Child("players").Child(user.UserId).Child("login")
                      .SetValueAsync(newEmail)
                      .ContinueWithOnMainThread(dbTask =>
                      {
                          if (dbTask.IsFaulted || dbTask.IsCanceled)
                          {
                              Debug.LogWarning("[PlayerManager] �chec mise � jour login distant: " + dbTask.Exception);
                          }
                          onSuccess?.Invoke();
                      });
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[PlayerManager] Exception lors maj BDD: " + ex);
                    onSuccess?.Invoke();
                }
            });
        });
    }

    public void UpdatePassword(string newPassword, string currentPassword, Action onSuccess, Action<Exception> onError)
    {
        if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
        {
            onError?.Invoke(new Exception("Nouveau mot de passe invalide (min 6 caract�res)."));
            return;
        }

        var auth = FirebaseAuth.DefaultInstance;
        var user = auth.CurrentUser;
        if (user == null)
        {
            onError?.Invoke(new Exception("Aucun utilisateur connect�."));
            return;
        }
        if (string.IsNullOrEmpty(currentPassword))
        {
            onError?.Invoke(new Exception("Mot de passe actuel requis pour r�-authentification."));
            return;
        }

        var credential = EmailAuthProvider.GetCredential(user.Email, currentPassword);

        user.ReauthenticateAsync(credential).ContinueWithOnMainThread(reauthTask =>
        {
            if (reauthTask.IsFaulted || reauthTask.IsCanceled)
            {
                onError?.Invoke(reauthTask.Exception ?? new Exception("R�-authentification �chou�e."));
                return;
            }

            user.UpdatePasswordAsync(newPassword).ContinueWithOnMainThread(updateTask =>
            {
                if (updateTask.IsFaulted || updateTask.IsCanceled)
                {
                    onError?.Invoke(updateTask.Exception ?? new Exception("�chec mise � jour mot de passe."));
                    return;
                }

                onSuccess?.Invoke();
            });
        });
    }
}
