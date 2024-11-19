using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int playerBalance = 0;
    private int matchCount = 0;
    private int initialBalance = 0;
    private bool hasFinished = false; // Флаг для отслеживания завершения

    [Header("Finish Settings")]
    public GameObject finishLine; // Назначьте через Inspector
    public UIManager uiManager; // Назначьте через Inspector

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Сохраняем объект при смене сцен
            SceneManager.sceneLoaded += OnSceneLoaded; // Подписываемся на событие
            Debug.Log("GameManager Instance Set");
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("Duplicate GameManager Destroyed");
        }
    }

    private void OnDestroy()
    {
        // Отписываемся от события при уничтожении GameManager
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Обновляем ссылки на объекты сцены
        FindUIManager();
        FindFinishLine();

        // Сбрасываем параметры игры
        hasFinished = false;
        matchCount = 0;
        Time.timeScale = 1f;
        CoinsPresset();

        Debug.Log("Параметры игры сброшены при загрузке сцены.");
    }

    private void SetupExitButton()
    {
        // Ищем объект с именем "Exit" в текущей сцене
        GameObject exitButtonObject = GameObject.Find("Exit");

        if (exitButtonObject != null)
        {
            // Получаем компонент Button на найденном объекте
            Button exitButton = exitButtonObject.GetComponent<Button>();

            if (exitButton != null)
            {
                // Добавляем метод ExitToMainMenu() в события OnClick
                exitButton.onClick.RemoveAllListeners(); // Очищаем предыдущие слушатели, если есть
                exitButton.onClick.AddListener(ExitToMainMenu);

                Debug.Log("Кнопка Exit настроена успешно.");
            }
        }
    }

    private void FindUIManager()
    {
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
    }

   private void FindFinishLine()
    {
        if (finishLine == null)
        {
            Transform parent = GameObject.Find("FinishParent")?.transform; // Замените "FinishParent" на имя родительского объекта
            if (parent != null)
            {
                finishLine = parent.Find("FinishLine")?.gameObject; // Замените "FinishLine" на имя вашего объекта
            }
        }
    }


    void Start()
    {
       CoinsPresset();
    }

    public void CoinsPresset()
    {
         // Проверяем, если сцена без UIManager, пропускаем дальнейшую логику
        if (uiManager == null)
        {
            Debug.Log("UIManager отсутствует. Пропускаем логику для этой сцены.");
            return;
        }

        // Инициализируем баланс из DataManager
        if (DataManager.Instance != null)
        {
            // Получаем выбранную сложность
            string selectedDifficulty = DataManager.Instance.selectedDifficulty;

            // Определяем стоимость входа
            int entryCost = 0;
            switch (selectedDifficulty)
            {
                case "Easy":
                    entryCost = 50;
                    break;
                case "Normal":
                    entryCost = 150;
                    break;
                case "Hard":
                    entryCost = 200;
                    break;
                default:
                    Debug.LogWarning("Неизвестная сложность. Устанавливаем стоимость входа на 50.");
                    entryCost = 50;
                    break;
            }

            // Проверяем, достаточно ли монет для начала игры
            if (DataManager.Instance.coins >= entryCost)
            {
                // Списываем стоимость входа
                bool deducted = DataManager.Instance.DeductCoins(entryCost);
                if (deducted)
                {
                    playerBalance = entryCost;
                    initialBalance = playerBalance;
                    Debug.Log($"Игра начата с балансом: {playerBalance} монет (стоимость входа: {entryCost} монет).");
                }
                else
                {
                    Debug.LogError("Не удалось списать стоимость входа. Переход в главное меню.");
                    SceneManager.LoadScene("MainMenu");
                    return;
                }
            }
            else
            {
                Debug.LogWarning("Недостаточно монет для начала игры. Переход в главное меню.");
                uiManager?.ShowNotification($"Недостаточно монет для начала {selectedDifficulty} сложности. Требуется {entryCost} монет.");
                StartCoroutine(ReturnToMainMenuAfterNotification("MainMenu"));
                return;
            }

            // Инициализация UI
            uiManager.UpdateScore(playerBalance);

            // Убедитесь, что FinishLine изначально деактивирован
            if (finishLine != null)
            {
                finishLine.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("DataManager.Instance не найден в GameManager.");
        }
    }

    // Метод для возврата в главное меню после уведомления
    IEnumerator ReturnToMainMenuAfterNotification(string sceneName)
    {
        // Предполагаем, что уведомление закрывается автоматически через некоторое время
        yield return new WaitForSeconds(2f); // Ждём 2 секунды
        SceneManager.LoadScene(sceneName);
    }

    // Метод для обновления баланса игрока
    public void UpdateBalance(int amount)
    {
        playerBalance += amount;
        Debug.Log($"Баланс игрока обновлён: {playerBalance}");
        // Обновляем UI, только если UIManager существует
        if (uiManager != null)
        {
            uiManager.UpdateScore(playerBalance);
        }
    }

    // Метод для применения эффекта
    public void ApplyEffect(Box.EffectType effectType, float effectValue)
    {
        switch (effectType)
        {
            case Box.EffectType.Add:
                playerBalance += (int)effectValue;
                break;
            case Box.EffectType.Multiply:
                playerBalance = (int)(playerBalance * effectValue);
                break;
            case Box.EffectType.Subtract:
                playerBalance -= (int)effectValue;
                break;
            case Box.EffectType.Divide:
                if (effectValue != 0)
                    playerBalance = (int)(playerBalance / effectValue);
                break;
        }
        Debug.Log($"Баланс игрока после эффекта: {playerBalance}");
        // Обновляем UI
        if (uiManager != null)
        {
            uiManager.UpdateScore(playerBalance);
        }
        else
        {
            Debug.LogError("UIManager не назначен в GameManager.");
        }
    }

    // Метод для инкрементации количества совпадений
    public void IncrementMatchCount()
    {
        if (hasFinished)
            return; // Если игра уже завершена, не увеличиваем счётчик

        matchCount++;
        Debug.Log($"Количество совпадений: {matchCount}");
        if (matchCount >= 4)
        {
            TriggerFinishSequence();
        }
    }

    // Метод для запуска финальной последовательности
    void TriggerFinishSequence()
{
    if (hasFinished)
        return; // Предотвращаем повторные вызовы

    Debug.Log("Достигнуто 4 совпадения. Запуск финальной последовательности.");

    // Найти текущий Ball (обычный шарик)
    Ball currentBall = FindObjectOfType<Ball>();
    if (currentBall != null)
    {
        // Заменяем обычный шарик на финальный
        BallSpawner.Instance.ReplaceBallWithFinalBall(currentBall);
    }
    else
    {
        Debug.LogError("Текущий Ball не найден в сцене.");
    }

    // Активируем FinishLine
    if (finishLine != null)
    {
        finishLine.SetActive(true);
        Debug.Log("FinishLine активирован.");
    }
    else
    {
        Debug.LogError("FinishLine не назначен в GameManager.");
    }

    // Сброс счетчика совпадений
    matchCount = 0;
    hasFinished = false;
    Debug.Log("Счетчик совпадений обнулен.");
}

    // Метод, вызываемый при достижении финальной полоски
    public void TriggerGameEnd()
    {
        if (hasFinished) return;

        Debug.Log("Завершение игры.");
        hasFinished = true;

        // Показать FinishPanel через UIManager, если он существует
        if (uiManager != null)
        {
            uiManager.ShowFinishPanel(playerBalance, initialBalance);
            SetupExitButton();
        }

        // Вычисляем заработанные монеты
        int earnedCoins = playerBalance;
        earnedCoins = Mathf.Max(earnedCoins, 0);

        if (DataManager.Instance != null)
        {
            DataManager.Instance.AddCoins(earnedCoins);
        }

        if (finishLine != null)
        {
            finishLine.SetActive(false);
        }

        Time.timeScale = 0f;
    }

    // Метод для перезапуска игры
    public void RestartGame()
    {
        Time.timeScale = 1f; // Восстанавливаем время
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Метод для выхода в главное меню
    public void ExitToMainMenu()
    {
        Time.timeScale = 1f; // Восстанавливаем время
        SceneManager.LoadScene("MainMenu"); // Убедитесь, что сцена "MainMenuScene" добавлена в Build Settings
    }

    // Метод для проверки, завершена ли игра
    public bool HasFinished()
    {
        return hasFinished;
    }
}
