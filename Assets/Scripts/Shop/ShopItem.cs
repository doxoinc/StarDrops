using UnityEngine;
using UnityEngine.UI;
using System;

public class ShopItem : MonoBehaviour
{
    [Header("Skin Data")]
    public string skinName; // Уникальное имя скина
    public Sprite skinSprite; // Изображение скина
    public int price; // Цена скина

    [Header("UI Elements")]
    public Image itemImage; // Изображение скина
    public Text priceText; // Текст с ценой
    public Button buyButton; // Кнопка покупки

    [Header("Optional UI")]
    public GameObject noCoinsPanel; // Панель уведомления о недостатке монет
    public static event Action OnItemPurchased;

    void Start()
    {
        // Настройка UI элементов
        if (itemImage != null)
        {
            itemImage.sprite = skinSprite;
            Debug.Log($"ShopItem: Назначен спрайт для {skinName}.");
        }
        else
        {
            Debug.LogError("ItemImage не назначен в ShopItem.");
        }

        if (priceText != null)
        {
            priceText.text = $"{price} Coins";
            Debug.Log($"ShopItem: Назначен текст цены для {skinName}: {price} Coins.");
        }
        else
        {
            Debug.LogError("PriceText не назначен в ShopItem.");
        }

        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnBuyButtonClicked);
            UpdateButton();
            Debug.Log($"ShopItem: Настроена кнопка покупки для {skinName}.");
        }
        else
        {
            Debug.LogError("BuyButton не назначен в ShopItem.");
        }
    }

    // Метод, вызываемый при нажатии кнопки покупки
    public void OnBuyButtonClicked()
    {
        Debug.Log($"Попытка покупки скина: {skinName} за {price} монет.");

        // Проверяем, не куплен ли уже скин
        if (DataManager.Instance.ownedSkins.Contains(skinName))
        {
            Debug.Log("Скин уже куплен. Выбор скина.");
            DataManager.Instance.SelectSkin(skinName);
            UpdateButton();
            ShopManager.Instance.UpdateYourBallsPanel();
            // Воспроизводим звук выбора (можно использовать PlayButtonClick или отдельный звук)
            AudioManager.Instance.PlayButtonClick();
            return;
        }

        // Пытаемся купить скин
        bool success = DataManager.Instance.PurchaseSkin(skinName, price);
        if (success)
        {
            Debug.Log("Покупка успешна.");
            // Обновляем UI
            UpdateButton();
            ShopManager.Instance.UpdateYourBallsPanel();
            // Воспроизводим звук покупки
            AudioManager.Instance.PlayPurchaseSound();
            OnItemPurchased?.Invoke();
        }
        else
        {
            Debug.Log("Покупка не удалась.");
            // Показываем уведомление о недостатке монет
            if (noCoinsPanel != null)
                noCoinsPanel.SetActive(true);
            else
                Debug.LogWarning("NoCoinsPanel не назначен в ShopItem.");
        }
    }

    // Метод для обновления состояния кнопки покупки
    public void UpdateButton()
    {
        if (DataManager.Instance.ownedSkins.Contains(skinName))
        {
            buyButton.interactable = false;
            Text buttonText = buyButton.GetComponentInChildren<Text>();
            if (buttonText != null)
                buttonText.text = "Owned";
            else
                Debug.LogWarning("Текст кнопки не найден в BuyButton.");
        }
        else
        {
            buyButton.interactable = true;
            Text buttonText = buyButton.GetComponentInChildren<Text>();
            if (buttonText != null)
                buttonText.text = "Buy";
            else
                Debug.LogWarning("Текст кнопки не найден в BuyButton.");
        }
    }
}
