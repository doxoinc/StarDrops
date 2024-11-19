using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    // Ссылки на панели
    [Header("Panels")]
    public GameObject challengePanel;
    public GameObject settingsPanel;
    public GameObject playPanel;
    public GameObject miniGamesPanel;
    public GameObject dailyRewardsPanel;
    public GameObject statisticsPanel;
    public GameObject shopPanel;

    // Ссылки на основные UI элементы
    [Header("Main UI")]
    public GameObject mainUI; // Контейнер с кнопками и текстом монет/звёзд
    public Text coinsText;
    public Text starsText;

    // Список всех панелей для удобства
    private List<GameObject> allPanels;



    void Start()
    {
        // Инициализация списка панелей
        allPanels = new List<GameObject>
        {
            challengePanel,
            settingsPanel,
            playPanel,
            miniGamesPanel,
            dailyRewardsPanel,
            statisticsPanel,
            shopPanel
        };

        // Обновляем UI
        UpdateUI();

        // Убедимся, что все панели закрыты при старте
        CloseAllPanels();
    }

    // Метод для обновления UI
    public void UpdateUI()
    {
        if (DataManager.Instance == null)
        {
            Debug.LogError("DataManager.Instance is null. Убедитесь, что DataManager присутствует в сцене и инициализирован.");
            return;
        }

        if (coinsText == null)
        {
            Debug.LogError("coinsText не назначен в MainMenuManager.");
            return;
        }

        if (starsText == null)
        {
            Debug.LogError("starsText не назначен в MainMenuManager.");
            return;
        }

        coinsText.text = DataManager.Instance.coins.ToString();
        starsText.text = DataManager.Instance.stars.ToString();
    }

    // Метод для открытия панели
    public void OpenPanel(GameObject panel)
    {
        if (panel == null)
        {
            Debug.LogError("Panel is null в OpenPanel.");
            return;
        }

        // Отключаем основное меню
        mainUI.SetActive(false);

        // Закрываем все панели
        CloseAllPanels();

        // Активируем нужную панель
        panel.SetActive(true);

        // Воспроизводим анимацию открытия, если есть Animator
        Animator animator = panel.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Open");
        }
    }

    // Метод для закрытия панели
    public void ClosePanel(GameObject panel)
    {
        if (panel == null)
        {
            Debug.LogError("Panel is null в ClosePanel.");
            return;
        }

        // Воспроизводим анимацию закрытия, если есть Animator
        Animator animator = panel.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Close");
            // Деактивируем панель после завершения анимации
            StartCoroutine(DeactivatePanelAfterAnimation(panel, animator));
        }
        else
        {
            // Если анимации нет, просто деактивируем панель
            panel.SetActive(false);
        }

        // Включаем основное меню
        mainUI.SetActive(true);
    }

    // Корутина для деактивации панели после завершения анимации
    private System.Collections.IEnumerator DeactivatePanelAfterAnimation(GameObject panel, Animator animator)
    {
        // Получаем продолжительность анимации закрытия
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;

        // Ждем окончания анимации (можно заменить на animationLength)
        yield return new WaitForSeconds(animationLength);

        panel.SetActive(false);
    }

    // Метод для закрытия всех панелей
    private void CloseAllPanels()
    {
        foreach (GameObject panel in allPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }

    // Методы, привязанные к кнопкам
    public void OnChallengeButton()
    {
        OpenPanel(challengePanel);
    }

    public void OnSettingsButton()
    {
        OpenPanel(settingsPanel);
    }

    public void OnPlayButton()
    {
        OpenPanel(playPanel);
    }

    public void OnMiniGamesButton()
    {
        OpenPanel(miniGamesPanel);
    }

    public void OnDailyRewardsButton()
    {
        OpenPanel(dailyRewardsPanel);
    }

    public void OnStatisticsButton()
    {
        OpenPanel(statisticsPanel);
    }

    public void OnShopButton()
    {
        OpenPanel(shopPanel);
    }

    // Методы для кнопок закрытия в панелях
    public void OnCloseButton(GameObject panel)
    {
        ClosePanel(panel);
    }

    // Метод для обновления UI при возвращении в главное меню
    void OnEnable()
    {
        UpdateUI();
        ShopItem.OnItemPurchased += UpdateUI;
    }
}
