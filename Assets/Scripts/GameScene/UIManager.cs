using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Для использования SceneManager
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Score UI")]
    public Text scoreText;

    [Header("Finish Panel")]
    public GameObject finishPanel;
    public Text finalScoreText;
    public Text starsText; // Текстовое отображение звёзд
    public Button finishBackButton;

    [Header("Finish Panel Animator")]
    public Animator finishPanelAnimator; // Ссылка на Animator панели завершения

    // Длительность анимации закрытия (если не извлекается из Animator)
    [Header("Animation Settings")]
    public float closeAnimationDelay = 0.5f; // По умолчанию 0.5 секунды

    void Start()
    {
        // Изначально скрываем панель завершения
        if (finishPanel != null)
        {
            finishPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("FinishPanel не назначен в UIManager.");
        }

        // Настраиваем кнопку закрытия панели завершения
        if (finishBackButton != null)
        {
            finishBackButton.onClick.AddListener(OnCloseFinishPanel);
        }
        else
        {
            Debug.LogError("FinishBackButton не назначен в UIManager.");
        }

        // Получаем Animator компонента finishPanel, если он не назначен
        if (finishPanelAnimator == null && finishPanel != null)
        {
            finishPanelAnimator = finishPanel.GetComponent<Animator>();
            if (finishPanelAnimator == null)
            {
                Debug.LogError("Animator не найден на FinishPanel.");
            }
        }

        // Проверка наличия starsText
        if (starsText == null)
        {
            Debug.LogError("StarsText не назначен в UIManager.");
        }
    }

    // Метод для обновления счёта на UI
    public void UpdateScore(float score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"{Mathf.FloorToInt(score)}";
        }
        else
        {
            Debug.LogError("ScoreText не назначен в UIManager.");
        }
    }

    // Метод для отображения панели завершения игры с анимацией
    public void ShowFinishPanel(float finalScore, float initialScore)
    {
        if (finishPanel != null && finishPanelAnimator != null)
        {
            // Устанавливаем финальный счёт
            if (finalScoreText != null)
            {
                finalScoreText.text = $"{Mathf.FloorToInt(finalScore)}"; // Только число
                Debug.Log($"Final Score Displayed: {finalScoreText.text}");
            }
            else
            {
                Debug.LogError("FinalScoreText не назначен в UIManager.");
            }

            // Вычисляем и отображаем количество звёзд
            int stars = CalculateStars(finalScore, initialScore);
            DisplayStars(stars);

            // Активируем панель перед запуском анимации
            finishPanel.SetActive(true);
            Debug.Log("FinishPanel активирован.");

            // Запускаем анимацию открытия панели
            finishPanelAnimator.SetTrigger("Open");
            Debug.Log("Триггер 'Open' установлен для FinishPanel Animator.");
        }
        else
        {
            Debug.LogError("FinishPanel или FinishPanelAnimator не назначены в UIManager.");
        }
    }

    // Метод для расчёта количества звёзд
    int CalculateStars(float finalScore, float initialScore)
    {
        if (initialScore == 0)
            initialScore = 1; // Избегаем деления на ноль

        float multiplier = finalScore / initialScore;
        int starCount = 0;

        if (multiplier >= 2f)
            starCount = 1;
        if (multiplier >= 4f)
            starCount = 2;
        if (multiplier >= 6f)
            starCount = 3;

        starCount = Mathf.Clamp(starCount, 0, 3);
        Debug.Log($"Stars Calculated: {starCount}");
        return starCount;
    }

    // Метод для отображения звёзд
    void DisplayStars(int starCount)
    {
        if (starsText != null)
        {
            starsText.text = $"{starCount}"; // Только число
            Debug.Log($"Stars Displayed: {starsText.text}");
        }
        else
        {
            Debug.LogError("StarsText не назначен в UIManager.");
        }

        // Обновление звёзд в DataManager
        if (DataManager.Instance != null)
        {
            DataManager.Instance.AddStars(starCount);
            Debug.Log($"Added {starCount} stars to DataManager. Total stars: {DataManager.Instance.stars}");
        }
        else
        {
            Debug.LogError("DataManager.Instance равен null. Убедитесь, что DataManager присутствует на сцене и реализует Singleton.");
        }
    }

    // Метод для закрытия панели завершения с анимацией
    public void OnCloseFinishPanel()
    {
        if (finishPanel != null && finishPanelAnimator != null)
        {
            // Запускаем анимацию закрытия панели
            finishPanelAnimator.SetTrigger("Close");
            Debug.Log("Триггер 'Close' установлен для FinishPanel Animator.");

            // Запускаем корутину для ожидания завершения анимации перед возвратом в главное меню
            StartCoroutine(WaitForCloseAnimation());
        }
        else
        {
            Debug.LogError("FinishPanel или FinishPanelAnimator не назначены в UIManager.");
        }
    }

    // Короутина для ожидания завершения анимации закрытия
    IEnumerator WaitForCloseAnimation()
    {
        // Получаем длительность анимации закрытия из Animator
        float closeAnimationLength = 0f;

        if (finishPanelAnimator.runtimeAnimatorController != null)
        {
            foreach (var clip in finishPanelAnimator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == "Close")
                {
                    closeAnimationLength = clip.length;
                    break;
                }
            }
        }

        // Если длительность анимации не найдена, используем стандартное время ожидания
        if (closeAnimationLength == 0f)
        {
            Debug.LogWarning("Длительность анимации Close не найдена. Используем стандартное время ожидания.");
            closeAnimationLength = closeAnimationDelay; // Используем заданное значение
        }

        // Ждём окончания анимации закрытия
        yield return new WaitForSeconds(closeAnimationLength);

        // Деактивируем панель после завершения анимации
        finishPanel.SetActive(false);
        Debug.Log("FinishPanel деактивирован после закрытия.");

        // Возвращаемся в главное меню
        SceneManager.LoadScene("MainMenu"); // Убедитесь, что такая сцена существует и правильно названа
        Debug.Log("Загружена сцена 'MainMenuScene'.");
    }

    // Additional method to update UI (e.g., stars)
    void UpdateUI()
    {
        // Find MainMenuManager and update UI
        MainMenuManager mainMenu = FindObjectOfType<MainMenuManager>();
        if (mainMenu != null)
        {
            mainMenu.UpdateUI();
        }
        else
        {
            Debug.LogWarning("MainMenuManager not found to update UI.");
        }
    }

    // Method to display notification with animation (if needed)
    public void ShowNotification(string message)
    {
        // Реализуйте логику показа уведомлений, если это необходимо
    }
}
