using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public enum StatType { Hp, Damage, Defense, Speed }

[Serializable]
public class StatUI
{
    public StatType statType;
    public TMP_Text valueText;
    public Button minusButton;
    public Button plusButton;
    public int step = 1;
}

public class PlayerUiController : MonoBehaviour
{
    public TMP_Text messageText;

    [Header("Référence Player")]
    public PlayerManager playerManager;

    [Header("Sauvegarde")]
    public Button saveButton;

    [Header("Login / Password")]
    public TMP_InputField loginInput;
    public TMP_Text loginValueText;
    public TMP_InputField passwordInput;
    public TMP_InputField newPasswordInput;

    [Header("Pseudo")]
    public TMP_Text pseudoText;
    public TMP_InputField pseudoInput;

    [Header("Stats (HP, Damage, Defense, Speed)")]
    public StatUI[] stats;

    [Header("Level Params")]
    public TMP_Text stageText;
    public TMP_Text levelText;
    public TMP_Text xpText;

    [Header("Son (bouton On/Off)")]
    public Button soundButton;
    public Image soundButtonImage;
    public Color soundOffColor = Color.red;
    public Color soundOnColor = Color.green;

    private bool authOperationInProgress = false;
    private bool hasUnsavedChanges = false;
    private string pendingNewPasswordValue = null;

    void Start()
    {
        playerManager = PlayerManager.Instance;

        if (playerManager == null)
        {
            Debug.LogError("PlayerUIController: playerManager non assigné !");
            return;
        }

        playerManager.OnPlayerDataChanged += OnPlayerChanged;

        SyncDataManager.EnsureInstanceFromResources();
        if (saveButton != null && SyncDataManager.Instance != null)
            saveButton.onClick.AddListener(OnSaveButtonClicked);

        // initialisation des listeners UI -> player
        //if (loginInput != null) loginInput.onEndEdit.AddListener(OnLoginInputEnd); // Plus utilisé car on ne modifie pas le login
        if (newPasswordInput != null) newPasswordInput.onEndEdit.AddListener(OnPasswordInputEnd);

        if (pseudoInput != null) pseudoInput.onEndEdit.AddListener(OnPseudoInputEnd);

        // Stat listeners
        foreach (var s in stats)
        {
            if (s == null) continue;

            var localStat = s;
            if (localStat.minusButton != null)
                localStat.minusButton.onClick.AddListener(() => OnStatChange(localStat.statType, -localStat.step));
            if (localStat.plusButton != null)
                localStat.plusButton.onClick.AddListener(() => OnStatChange(localStat.statType, localStat.step));
        }

        if (soundButton != null)
        {
            soundButton.onClick.AddListener(OnSoundButtonClicked);
        }

        // update UI initiale
        OnPlayerChanged(playerManager.data);
    }

    void OnDestroy()
    {
        if (saveButton != null) saveButton.onClick.RemoveListener(OnSaveButtonClicked);
        if (playerManager != null) playerManager.OnPlayerDataChanged -= OnPlayerChanged;
        //if (loginInput != null) loginInput.onEndEdit.RemoveListener(OnLoginInputEnd); // On ne modifie plus le login
        if (pseudoInput != null) pseudoInput.onEndEdit.RemoveListener(OnPseudoInputEnd);

        foreach (var s in stats)
        {
            if (s == null) continue;
            if (s.minusButton != null) s.minusButton.onClick.RemoveAllListeners();
            if (s.plusButton != null) s.plusButton.onClick.RemoveAllListeners();
        }

        if (soundButton != null) soundButton.onClick.RemoveListener(OnSoundButtonClicked);
    }

    // ---------------- UI -> Player ----------------
    void OnSaveButtonClicked()
    {
        if (authOperationInProgress || hasUnsavedChanges == false)
        {
            return;
        }

        if (!string.IsNullOrEmpty(pendingNewPasswordValue))
        {
            var currentPass = passwordInput != null ? passwordInput.text : null;
            if (string.IsNullOrEmpty(currentPass))
            {
                ShowMessage("Veuillez entrer votre mot de passe actuel pour confirmer le changement.");
                return;
            }

            SetAuthOperation(true);
            ShowMessage("Mise à jour du mot de passe en cours...");

            playerManager.UpdatePassword(pendingNewPasswordValue, currentPass,
                () =>
                {
                    ShowMessage("Mot de passe modifié. Sauvegarde des données...");
                    pendingNewPasswordValue = null;
                    // On vide les champs
                    if (newPasswordInput != null) newPasswordInput.text = "";
                    if (passwordInput != null) passwordInput.text = "";

                    SyncDataManager.Instance?.SaveAll();
                    
                    SetHasUnsavedChanges(false);
                    SetAuthOperation(false);
                    ShowMessage("Sauvegarde terminée et mot de passe mis à jour !");
                },
                ex =>
                {
                    SetAuthOperation(false);
                    ShowMessage("Erreur lors de la modification du mot de passe ");
                });

            return;
        }

        // Si on n'as pas de changement de MDP :
        SyncDataManager.EnsureInstanceFromResources();
        SyncDataManager.Instance.SaveAll();
        ShowMessage("Sauvegarde terminé !");
        SetHasUnsavedChanges(false);
    }

    void OnPasswordInputEnd(string newText)
    {
        var newPass = newPasswordInput != null ? newPasswordInput.text : null;
        if (string.IsNullOrEmpty(newPass))
        {
            pendingNewPasswordValue = null;
            return;
        }

        if (newPass.Length < 6)
        {
            ShowMessage("Nouveau mot de passe invalide (min 6 caractères).");
            pendingNewPasswordValue = null;
            return;
        }

        pendingNewPasswordValue = newPass;
        SetHasUnsavedChanges(true);
        ShowMessage("Nouveau mot de passe prêt. Cliquez sur Save pour appliquer.");
    }

    void OnPseudoInputEnd(string newText)
    {
        if (!string.IsNullOrEmpty(newText))
        {
            playerManager.SetPseudo(newText);
            SetHasUnsavedChanges(true);
        }
    }

    void OnStatChange(StatType type, int delta)
    {
        SetHasUnsavedChanges(true);
        switch (type)
        {
            case StatType.Hp:
                playerManager.ChangeHpMax(delta);
                break;
            case StatType.Damage:
                playerManager.ChangeDamage(delta);
                break;
            case StatType.Defense:
                playerManager.ChangeDefense(delta);
                break;
            case StatType.Speed:
                playerManager.ChangeSpeed(delta);
                break;
        }
    }

    void OnSoundButtonClicked()
    {
        if (playerManager != null)
        {
            playerManager.ToggleVolumeSound();
            SetHasUnsavedChanges(true);
        }
    }

    // ---------------- Player -> UI ----------------
    void OnPlayerChanged(PlayerData d)
    {
        if (d == null) return;

        // Pseudo
        if (pseudoText != null) pseudoText.text = d.pseudo ?? "";
        if (pseudoInput != null)
        {
            var ph = pseudoInput.placeholder as TMP_Text;
            if (ph != null) ph.text = d.pseudo ?? "Pseudo";
        }

        // Login / Password
        if (loginInput != null)
        {
            var ph = loginInput.placeholder as TMP_Text;
            if (ph != null) ph.text = d.login ?? "Login";
        }
        if (loginValueText != null) loginValueText.text = d.login.ToString();

        // Stats :
        foreach (var s in stats)
        {
            if (s == null || s.valueText == null) continue;
            switch (s.statType)
            {
                case StatType.Hp:
                    s.valueText.text = $"{d.hp}/{d.hpMax}";
                    break;
                case StatType.Damage:
                    s.valueText.text = d.damage.ToString();
                    break;
                case StatType.Defense:
                    s.valueText.text = d.defense.ToString();
                    break;
                case StatType.Speed:
                    s.valueText.text = d.speed.ToString();
                    break;
            }
        }

        if (soundButtonImage != null)
        {
            if (d.volumeSound == true)
            {
                soundButtonImage.color = soundOnColor;
            }
            else
            {
                soundButtonImage.color = soundOffColor;
            }
        }

        // Level Params
        if (stageText != null) stageText.text = d.stage.ToString();
        if (levelText != null) levelText.text = d.level.ToString();
        if (xpText != null) xpText.text = d.xp.ToString();
    }

    private void ShowMessage(string m)
    {
        if (messageText != null) messageText.text = m;
        Debug.Log("[PlayerUIController] " + m);
    }

    private void SetAuthOperation(bool value)
    {
        authOperationInProgress = value;
    }

    private void SetHasUnsavedChanges(bool value)
    {
        hasUnsavedChanges = value;
    }
}
