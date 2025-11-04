using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public Button resumeButton;
    public Button saveButton;
    public Button mainMenuButton;
    private bool isPaused = false;

    PlayerManager playerManager;

    [Header("Player Stats in Pause Menu")]
    public Button hpMinusButton;
    public Button hpPlusButton;
    public Button expMinusButton;
    public Button expPlusButton;
    public Button stageMinusButton;
    public Button stagePlusButton;
    public Button confirmStage;
    public TMP_Text hpText;
    public TMP_Text xpText;
    public TMP_Text stageText;
    public TMP_Text levelText;

    private int xpGoal;

    private void Start()
    {
        playerManager = PlayerManager.Instance;

        if (pauseMenuUI != null)
        {
            SetupPauseMenu();
        }
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                isPaused = false;
                ResumeGame();
            }
            else
            {
                UpdatePlayerStatsUI();
                isPaused = true;
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        PlayerInput playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput != null) playerInput.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UpdatePlayerStatsUI();
    }

    public void SetupPauseMenu()
    {
        pauseMenuUI.SetActive(false);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (saveButton != null)
            saveButton.onClick.AddListener(SaveGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        if (hpMinusButton != null)
            hpMinusButton.onClick.AddListener(() => OnHpChange(-10));
        if (hpPlusButton != null)
            hpPlusButton.onClick.AddListener(() => OnHpChange(10));

        if (expMinusButton != null)
            expMinusButton.onClick.AddListener(() => OnXpChange(-50));
        if (expPlusButton != null)
            expPlusButton.onClick.AddListener(() => OnXpChange(50));

        if (stageMinusButton != null)
            stageMinusButton.onClick.AddListener(() => OnStageChange(-1));
        if (stagePlusButton != null)
            stagePlusButton.onClick.AddListener(() => OnStageChange(1));

        if(confirmStage != null)
            confirmStage.onClick.AddListener(()=> OnSelectedStage(playerManager.data.stage));
    }

    public void SaveGame()
    {
        SyncDataManager.Instance?.SaveAll();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        PlayerInput playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput != null) playerInput.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UpdatePlayerStatsUI()
    {
        // HP
        if (hpText != null)
            hpText.text = $"{playerManager.data.hp}/{playerManager.data.hpMax}";

        // XP
        if (xpText != null)
            xpText.text = $"{playerManager.data.xp}/{xpGoal}";

        // Stage
        if (stageText != null)
            stageText.text = playerManager.data.stage.ToString();

        // Level
        if (levelText != null)
            levelText.text = playerManager.data.level.ToString();
    }

    private void OnHpChange(int delta)
    {

        if (delta > 0)
        {
            playerManager.data.hp = playerManager.data.hp + delta;
        }
        else
        {
            int newHp = Mathf.Max(1, playerManager.data.hp + delta);
            playerManager.data.hp = newHp;

            if (playerManager.data.hp > newHp)
            {
                playerManager.data.hp = newHp;
            }
        }

        UpdatePlayerStatsUI();
    }

    private void OnXpChange(int delta)
    {
        xpGoal = (int)(((playerManager.data.level * 100) * 1.5) + playerManager.data.level * 100);

        int newXp = playerManager.data.xp + delta;

        if (delta > 0)
        {
            while (newXp >= xpGoal && delta > 0)
            {
                playerManager.data.level++;
                int surplus = newXp - xpGoal;

                xpGoal = (int)(((playerManager.data.level * 100) * 1.5) + playerManager.data.level * 100);

                newXp = surplus;

                delta = surplus;
            }

            playerManager.data.xp = Mathf.Max(0, newXp);
        }
        else if (delta < 0)
        {
            while (newXp < 0 && playerManager.data.level > 1)
            {
                playerManager.data.level--;

                xpGoal = (int)(((playerManager.data.level * 100) * 1.5) + playerManager.data.level * 100);

                newXp += xpGoal;
            }

            playerManager.data.xp = Mathf.Max(0, newXp);
        }
        UpdatePlayerStatsUI();
    }

    private void OnStageChange(int delta)
    {
        int newStage = Mathf.Clamp(playerManager.data.stage + delta, 1, 3);

        if (newStage != playerManager.data.stage)
        {
            playerManager.data.stage = newStage;
            UpdatePlayerStatsUI();
        }
    }

    private void OnSelectedStage(int index)
    {
        SceneManager.LoadScene($"Stage" + (index), LoadSceneMode.Single);
    }
}