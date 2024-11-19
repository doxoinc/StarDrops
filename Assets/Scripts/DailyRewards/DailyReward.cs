using UnityEngine;
using UnityEngine.UI;
using System;

public class DailyReward : MonoBehaviour
{
    [Header("UI Elements")]
    public Button[] dayButtons; // Кнопки для каждого дня
    public Sprite claimedSprite; // Изображение для кнопки после получения награды

    private const string LastClaimKey = "LastDailyRewardClaim";
    private const string CurrentDayKey = "CurrentDailyRewardDay";

    private int[] rewards = { 150, 200, 250, 300, 350, 1000 }; // Награды за каждый день

    void Start()
    {
        InitializeButtons();
    }

    void InitializeButtons()
    {
        int currentDay = PlayerPrefs.GetInt(CurrentDayKey, 1); // Текущий доступный день
        string lastClaimString = PlayerPrefs.GetString(LastClaimKey, "0");

        DateTime lastClaimDate = DateTime.FromBinary(Convert.ToInt64(lastClaimString));
        bool canClaimToday = (DateTime.Now.Date - lastClaimDate.Date).TotalDays >= 1;

        for (int i = 0; i < dayButtons.Length; i++)
        {
            Button button = dayButtons[i];
            Image buttonImage = button.GetComponent<Image>();

            // Устанавливаем обработчик только для текущего дня
            if (i + 1 == currentDay && canClaimToday)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => ClaimReward(i));
            }

            // Блокируем кнопку только если награда уже была получена
            if (i + 1 < currentDay)
            {
                buttonImage.sprite = claimedSprite; // Устанавливаем спрайт "получено"
            }
            else
            {
                button.interactable = true; // Остальные кнопки интерактивны
            }
        }
    }

    public void ClaimReward(int dayIndex)
    {
        if (dayIndex < 0 || dayIndex >= rewards.Length) return;

        int rewardCoins = rewards[dayIndex];
        DataManager.Instance.AddCoins(rewardCoins);

        // Обновление UI для нажатой кнопки
        Button currentButton = dayButtons[dayIndex];
        Image buttonImage = currentButton.GetComponent<Image>();
        currentButton.interactable = false;
        buttonImage.sprite = claimedSprite;

        // Обновление данных
        PlayerPrefs.SetInt(CurrentDayKey, dayIndex + 2); // Открываем следующий день
        PlayerPrefs.SetString(LastClaimKey, DateTime.Now.ToBinary().ToString());
        PlayerPrefs.Save();

        InitializeButtons(); // Обновляем состояние кнопок
    }
}
