using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("Shop Settings")]
    public GameObject shopItemPrefab; // Префаб ShopItem
    public Transform shopItemsContainer; // Контейнер для ShopItems

    [Header("YourBalls Panel")]
    public Image currentBallImage1; // Image для YourBallsPanel 1
    public Image currentBallImage2; // Image для YourBallsPanel 2

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Optionally, DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("Duplicate ShopManager destroyed.");
        }
    }

    void Start()
    {
        PopulateShop();
        UpdateYourBallsPanel();

        // Подписываемся на событие изменения скина
        if (DataManager.Instance != null)
            DataManager.Instance.OnSkinChanged += UpdateYourBallsPanel;
        else
            Debug.LogError("DataManager.Instance равен null. Убедитесь, что DataManager присутствует на сцене.");
    }

    void OnDestroy()
    {
        if (DataManager.Instance != null)
            DataManager.Instance.OnSkinChanged -= UpdateYourBallsPanel;
    }

    // Метод для заполнения магазина доступными скинами
    void PopulateShop()
    {
        Debug.Log("ShopManager: Начинаем заполнение магазина.");
        foreach (var skinData in DataManager.Instance.allSkins)
        {
            Debug.Log($"ShopManager: Обрабатываем скин {skinData.skinName} с ценой {skinData.price}.");
            
            // Пропускаем бесплатные скины, если они уже в ownedSkins
            if (skinData.price == 0 && !DataManager.Instance.ownedSkins.Contains(skinData.skinName))
            {
                Debug.Log($"ShopManager: Добавляем бесплатный скин {skinData.skinName} в ownedSkins.");
                DataManager.Instance.ownedSkins.Add(skinData.skinName);
                DataManager.Instance.selectedSkin = skinData.skinName;
                DataManager.Instance.SaveData();
            }

            // Создаём объект ShopItem
            GameObject shopItemObj = Instantiate(shopItemPrefab, shopItemsContainer);
            ShopItem shopItem = shopItemObj.GetComponent<ShopItem>();
            if (shopItem != null)
            {
                shopItem.skinName = skinData.skinName;
                shopItem.skinSprite = skinData.skinSprite;
                shopItem.price = skinData.price;
                shopItem.UpdateButton();
                Debug.Log($"ShopManager: Создан ShopItem для {skinData.skinName}.");
            }
            else
            {
                Debug.LogError("ShopItem компонент не найден на префабе ShopItem.");
            }
        }
        Debug.Log("ShopManager: Завершено заполнение магазина.");
    }

    // Метод для обновления панели YourBallsPanel
    public void UpdateYourBallsPanel()
    {
        if (currentBallImage1 != null && currentBallImage2 != null)
        {
            string selectedSkin = DataManager.Instance.selectedSkin;
            Sprite selectedSprite = DataManager.Instance.GetSkinSprite(selectedSkin);

            // Если спрайт не найден, используем базовый спрайт
            if (selectedSprite == null)
            {
                Debug.LogWarning($"Спрайт для скина {selectedSkin} не найден. Используем базовый спрайт.");
                return;
            }

            // Обновляем изображения в YourBallsPanel
            currentBallImage1.sprite = selectedSprite;
            currentBallImage2.sprite = selectedSprite;

            Debug.Log($"YourBallsPanel обновлён скином: {selectedSkin}");
        }
        else
        {
            Debug.LogError("currentBallImage1 или currentBallImage2 не назначены в ShopManager.");
        }
    }
}
