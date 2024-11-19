using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Для использования SceneManager
using System.Collections;

[System.Serializable]
public struct DifficultyButtonSpritesUnlocked
{
    public Sprite normal;    // Sprite for unselected state
    public Sprite selected;  // Sprite for selected state
}

[System.Serializable]
public struct DifficultyButtonSpritesLockable
{
    public Sprite normal;    // Sprite for unselected and unlocked state
    public Sprite locked;    // Sprite for locked state
    public Sprite selected;  // Sprite for selected state
}

public class PlayPanelManager : MonoBehaviour
{
    // References to difficulty buttons
    [Header("Difficulty Buttons")]
    public Button easyButton;
    public Button normalButton;
    public Button hardButton;

    // Sprites for each difficulty button
    [Header("Sprites")]
    public DifficultyButtonSpritesUnlocked easyButtonSprites;
    public DifficultyButtonSpritesLockable normalButtonSprites;
    public DifficultyButtonSpritesLockable hardButtonSprites;

    // References to Image components of buttons for sprite changes
    private Image easyButtonImage;
    private Image normalButtonImage;
    private Image hardButtonImage;

    // Selected difficulty
    private string selectedDifficulty = "Easy"; // Default is Easy

    // Stars required for unlocking
    [Header("Stars Required for Unlocking")]
    public int starsForNormal = 50;
    public int starsForHard = 100;

    // Cost to start each difficulty mode
    [Header("Cost to Start Each Difficulty")]
    public int costEasy = 50;
    public int costNormal = 150;
    public int costHard = 200;

    // References to notification
    [Header("Notification")]
    public GameObject notificationPanel;
    public Text notificationText;
    public Button closeNotificationButton;

    // Reference to Animator for notification panel
    private Animator notificationAnimator;
    private bool isNotificationVisible = false;

    // Flag to prevent multiple coroutines for closing
    private bool isClosing = false;

    void Awake()
    {
        // Get Image components of buttons
        if (easyButton != null)
            easyButtonImage = easyButton.GetComponent<Image>();
        else
            Debug.LogError("easyButton is not assigned in PlayPanelManager.");

        if (normalButton != null)
            normalButtonImage = normalButton.GetComponent<Image>();
        else
            Debug.LogError("normalButton is not assigned in PlayPanelManager.");

        if (hardButton != null)
            hardButtonImage = hardButton.GetComponent<Image>();
        else
            Debug.LogError("hardButton is not assigned in PlayPanelManager.");

        // Check for null Image components
        if (easyButtonImage == null)
        {
            Debug.LogError("easyButtonImage not found. Ensure easyButton has an Image component.");
        }
        if (normalButtonImage == null)
        {
            Debug.LogError("normalButtonImage not found. Ensure normalButton has an Image component.");
        }
        if (hardButtonImage == null)
        {
            Debug.LogError("hardButtonImage not found. Ensure hardButton has an Image component.");
        }

        // Get Animator for notification panel
        if (notificationPanel != null)
        {
            notificationAnimator = notificationPanel.GetComponent<Animator>();
            if (notificationAnimator == null)
            {
                Debug.LogError("Animator не добавлен к notificationPanel.");
            }
        }
        else
        {
            Debug.LogWarning("notificationPanel is not assigned in PlayPanelManager.");
        }
    }

    void Start()
    {
        // Check if all sprites are assigned
        if (easyButtonSprites.normal == null || easyButtonSprites.selected == null)
        {
            Debug.LogError("Sprites for Easy Button are not assigned!");
        }
        if (normalButtonSprites.normal == null || normalButtonSprites.locked == null || normalButtonSprites.selected == null)
        {
            Debug.LogError("Sprites for Normal Button are not assigned!");
        }
        if (hardButtonSprites.normal == null || hardButtonSprites.locked == null || hardButtonSprites.selected == null)
        {
            Debug.LogError("Sprites for Hard Button are not assigned!");
        }

        // Setup difficulty buttons
        SetupDifficultyButtons();

        // Select saved difficulty or default to Easy
        if (DataManager.Instance != null)
        {
            selectedDifficulty = DataManager.Instance.selectedDifficulty;
            SelectDifficulty(selectedDifficulty);
        }
        else
        {
            Debug.LogError("DataManager.Instance is null in Start().");
        }

        // Setup close notification button
        if (closeNotificationButton != null)
        {
            closeNotificationButton.onClick.AddListener(OnCloseNotification);
        }
        else
        {
            Debug.LogWarning("closeNotificationButton is not assigned in PlayPanelManager.");
        }

        // Initially hide the notification panel (animated)
        if (notificationPanel != null && notificationAnimator != null)
        {
            // Assume the notification panel is hidden at start
            notificationPanel.SetActive(false);
            isNotificationVisible = false;
        }
        else
        {
            if (notificationPanel == null)
                Debug.LogWarning("notificationPanel is not assigned in PlayPanelManager.");
            if (notificationAnimator == null)
                Debug.LogWarning("notificationAnimator is not assigned in PlayPanelManager.");
        }
    }

    void SetupDifficultyButtons()
    {
        if (DataManager.Instance == null)
        {
            Debug.LogError("DataManager.Instance is null in SetupDifficultyButtons.");
            return;
        }

        // Easy button is always unlocked
        SetButtonStateUnlocked(easyButton, easyButtonImage, easyButtonSprites);

        // Normal button
        bool isNormalLocked = !DataManager.Instance.IsNormalUnlocked();
        SetButtonStateLockable(normalButton, normalButtonImage, normalButtonSprites, isLocked: isNormalLocked);

        // Hard button
        bool isHardLocked = !DataManager.Instance.IsHardUnlocked();
        SetButtonStateLockable(hardButton, hardButtonImage, hardButtonSprites, isLocked: isHardLocked);
    }

    void SetButtonStateUnlocked(Button button, Image buttonImage, DifficultyButtonSpritesUnlocked sprites)
    {
        if (button == null)
        {
            Debug.LogError("Button is null in SetButtonStateUnlocked.");
            return;
        }
        if (buttonImage == null)
        {
            Debug.LogError("buttonImage is null in SetButtonStateUnlocked.");
            return;
        }
        if (sprites.normal == null)
        {
            Debug.LogError("sprites.normal is null in SetButtonStateUnlocked.");
        }

        button.interactable = true;
        buttonImage.sprite = sprites.normal;
    }

    void SetButtonStateLockable(Button button, Image buttonImage, DifficultyButtonSpritesLockable sprites, bool isLocked)
    {
        if (button == null)
        {
            Debug.LogError("button is null in SetButtonStateLockable.");
            return;
        }
        if (buttonImage == null)
        {
            Debug.LogError("buttonImage is null in SetButtonStateLockable.");
            return;
        }

        // Always make the button interactable
        button.interactable = true;

        if (isLocked)
        {
            buttonImage.sprite = sprites.locked;
            // Optionally, make the button non-interactable if locked
            // button.interactable = false;
        }
        else
        {
            buttonImage.sprite = sprites.normal;
        }
    }

    // Method to select difficulty
    public void SelectDifficulty(string difficulty)
    {
        selectedDifficulty = difficulty;

        // Reset all button sprites to normal or locked
        ResetDifficultyButtons();

        // Set the selected button sprite
        switch (difficulty)
        {
            case "Easy":
                easyButtonImage.sprite = easyButtonSprites.selected;
                break;
            case "Normal":
                normalButtonImage.sprite = normalButtonSprites.selected;
                break;
            case "Hard":
                hardButtonImage.sprite = hardButtonSprites.selected;
                break;
            default:
                Debug.LogWarning("Unknown difficulty: " + difficulty);
                break;
        }

        // Save the selected difficulty
        if (DataManager.Instance != null)
        {
            DataManager.Instance.selectedDifficulty = selectedDifficulty;
            DataManager.Instance.SaveData();
        }
        else
        {
            Debug.LogError("DataManager.Instance is null when saving selected difficulty.");
        }
    }

    void ResetDifficultyButtons()
    {
        // Reset Easy button sprite
        if (easyButtonImage != null)
            easyButtonImage.sprite = easyButtonSprites.normal;

        // Reset Normal button sprite
        if (normalButtonImage != null)
        {
            if (DataManager.Instance != null && DataManager.Instance.IsNormalUnlocked())
                normalButtonImage.sprite = normalButtonSprites.normal;
            else
                normalButtonImage.sprite = normalButtonSprites.locked;
        }

        // Reset Hard button sprite
        if (hardButtonImage != null)
        {
            if (DataManager.Instance != null && DataManager.Instance.IsHardUnlocked())
                hardButtonImage.sprite = hardButtonSprites.normal;
            else
                hardButtonImage.sprite = hardButtonSprites.locked;
        }
    }

    // Method for Play button inside PlayPanel
    public void OnPlayButton()
    {
        if (DataManager.Instance == null)
        {
            Debug.LogError("DataManager.Instance is null in OnPlayButton.");
            ShowNotification("Error: Player data not found.");
            return;
        }

        int requiredCoins = 0;

        switch (selectedDifficulty)
        {
            case "Easy":
                requiredCoins = costEasy;
                break;
            case "Normal":
                requiredCoins = costNormal;
                break;
            case "Hard":
                requiredCoins = costHard;
                break;
            default:
                Debug.LogWarning("Unknown selected difficulty: " + selectedDifficulty);
                ShowNotification("Error: Unknown difficulty.");
                return;
        }

        if (DataManager.Instance.coins >= requiredCoins)
        {
            // Deduct coins
            bool deducted = DataManager.Instance.DeductCoins(requiredCoins);
            if (deducted)
            {
                // Load game scene
                SceneManager.LoadScene("GameScene");
            }
            else
            {
                Debug.LogError("Failed to deduct coins.");
                ShowNotification("Failed to deduct coins.");
            }
        }
        else
        {
            // Not enough coins
            ShowNotification($"Not enough coins to start {selectedDifficulty} mode. {requiredCoins} coins required.");
        }
    }

    // Method to update panel when reopening
    void OnEnable()
    {
        Debug.Log("PlayPanelManager OnEnable");
        SetupDifficultyButtons();
        UpdateSelectedSprite();
        UpdateSelectedButtonSprite();
    }

    void UpdateSelectedSprite()
    {
        if (DataManager.Instance == null)
        {
            Debug.LogError("DataManager.Instance is null in UpdateSelectedSprite.");
            return;
        }

        // Ensure the selected level is available
        if (selectedDifficulty == "Normal" && !DataManager.Instance.IsNormalUnlocked())
        {
            selectedDifficulty = "Easy";
        }
        if (selectedDifficulty == "Hard" && !DataManager.Instance.IsHardUnlocked())
        {
            if (DataManager.Instance.IsNormalUnlocked())
                selectedDifficulty = "Normal";
            else
                selectedDifficulty = "Easy";
        }

        SelectDifficulty(selectedDifficulty);
    }

    void UpdateSelectedButtonSprite()
    {
        // Update the selected button sprite
        switch (selectedDifficulty)
        {
            case "Easy":
                easyButtonImage.sprite = easyButtonSprites.selected;
                break;
            case "Normal":
                normalButtonImage.sprite = normalButtonSprites.selected;
                break;
            case "Hard":
                hardButtonImage.sprite = hardButtonSprites.selected;
                break;
            default:
                Debug.LogWarning("Unknown difficulty for UpdateSelectedButtonSprite: " + selectedDifficulty);
                break;
        }
    }

    // Methods for difficulty buttons
    public void OnEasyButton()
    {
        SelectDifficulty("Easy");
    }

    public void OnNormalButton()
    {
        if (DataManager.Instance == null)
        {
            Debug.LogError("DataManager.Instance is null in OnNormalButton.");
            ShowNotification("Error: Player data not found.");
            return;
        }

        if (DataManager.Instance.IsNormalUnlocked())
        {
            SelectDifficulty("Normal");
        }
        else if (DataManager.Instance.stars >= starsForNormal)
        {
            if (DataManager.Instance.UnlockNormal())
            {
                SetButtonStateLockable(normalButton, normalButtonImage, normalButtonSprites, isLocked: false);
                SelectDifficulty("Normal");
                ShowNotification("Normal level successfully unlocked!");
                UpdateUI(); // Update stars display
            }
            else
            {
                ShowNotification("Failed to unlock Normal level.");
            }
        }
        else
        {
            ShowNotification($"To unlock Normal, {starsForNormal} stars are required.");
        }
    }

    public void OnHardButton()
    {
        if (DataManager.Instance == null)
        {
            Debug.LogError("DataManager.Instance is null in OnHardButton.");
            ShowNotification("Error: Player data not found.");
            return;
        }

        if (DataManager.Instance.IsHardUnlocked())
        {
            SelectDifficulty("Hard");
        }
        else if (DataManager.Instance.stars >= starsForHard)
        {
            if (DataManager.Instance.UnlockHard())
            {
                SetButtonStateLockable(hardButton, hardButtonImage, hardButtonSprites, isLocked: false);
                SelectDifficulty("Hard");
                ShowNotification("Hard level successfully unlocked!");
                UpdateUI(); // Update stars display
            }
            else
            {
                ShowNotification("Failed to unlock Hard level.");
            }
        }
        else
        {
            ShowNotification($"To unlock Hard, {starsForHard} stars are required.");
        }
    }

    // Method to display notification with animation
    void ShowNotification(string message)
    {
        if (notificationPanel != null && notificationText != null && notificationAnimator != null)
        {
            notificationText.text = message;
            notificationPanel.SetActive(true); // Ensure the panel is active before triggering animation
            notificationAnimator.SetTrigger("Open"); // Trigger the open animation
            isNotificationVisible = true;
            Debug.Log($"Notification displayed: {message}");
        }
        else
        {
            Debug.LogWarning("NotificationPanel, NotificationText, or NotificationAnimator are not assigned.");
        }
    }

    // Method to close notification with animation
    public void OnCloseNotification()
    {
        if (notificationPanel != null && notificationAnimator != null && !isClosing)
        {
            notificationAnimator.SetTrigger("Close"); // Trigger the close animation
            isNotificationVisible = false;
            Debug.Log("Триггер 'Close' установлен для Notification Animator.");
            StartCoroutine(WaitForCloseAnimation());
        }
        else
        {
            if (notificationPanel == null || notificationAnimator == null)
                Debug.LogWarning("NotificationPanel or NotificationAnimator are not assigned.");
            if (isClosing)
                Debug.LogWarning("Closing notification is already in progress.");
        }
    }

    // Coroutine to wait for the close animation to finish and disable the panel
    IEnumerator WaitForCloseAnimation()
    {
        isClosing = true;

        // Get the duration of the "Close" animation
        float closeAnimationLength = 0f;

        // Find the "Close" animation clip in Animator
        foreach (var clip in notificationAnimator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == "Close")
            {
                closeAnimationLength = clip.length;
                break;
            }
        }

        // If duration not found, use a default wait time
        if (closeAnimationLength == 0f)
        {
            Debug.LogWarning("Close animation duration not found. Using default wait time.");
            //closeAnimationLength = closeAnimationDelay; // Use the set delay
        }

        // Wait for the close animation to finish
        yield return new WaitForSeconds(closeAnimationLength);

        // Disable the notification panel
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
            Debug.Log("NotificationPanel деактивирован после закрытия.");
        }

        isClosing = false;
    }

    // Additional method to update UI (e.g., stars)
    void UpdateUI()
    {
        // Find MainMenuManager and update UI
        MainMenuManager mainMenu = FindObjectOfType<MainMenuManager>();
        if (mainMenu != null)
        {
            mainMenu.UpdateUI();
            Debug.Log("MainMenuManager UI обновлён.");
        }
        else
        {
            Debug.LogWarning("MainMenuManager not found to update UI.");
        }
    }
}
