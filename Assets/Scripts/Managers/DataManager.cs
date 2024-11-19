using UnityEngine;
using System;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    // Пример данных
    public int coins;
    public int stars;
    public int gameLevel;
    public int miniGameLevel;
    public string lastDailyRewardClaim;

    // Новые переменные
    public string selectedDifficulty = "Easy";

    // Настройки звука и музыки
    public bool musicOn = true;
    public bool soundsOn = true;

    // Новые поля для магазина
    public List<string> ownedSkins = new List<string>(); // Список приобретённых скинов
    public string selectedSkin = "DefaultBall1"; // Выбранный скин

    // Список всех доступных скинов
    public List<SkinData> allSkins = new List<SkinData>();

    // События для уведомления об изменении скина
    public event Action OnSkinChanged;

    void Awake()
    {
        // Реализация паттерна Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
            Debug.Log("DataManager initialized.");
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("Duplicate DataManager destroyed.");
        }
    }

    // Класс для хранения данных о скине
    [System.Serializable]
    public class SkinData
    {
        public string skinName; // Уникальное имя скина
        public Sprite skinSprite; // Изображение скина
        public int price; // Цена скина
    }

    // Метод для сохранения данных
    public void SaveData()
    {
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("Stars", stars);
        PlayerPrefs.SetInt("GameLevel", gameLevel);
        PlayerPrefs.SetInt("MiniGameLevel", miniGameLevel);
        PlayerPrefs.SetString("LastDailyRewardClaim", lastDailyRewardClaim);

        // Сохраняем состояние разблокированных уровней
        PlayerPrefs.SetInt("NormalUnlocked", IsNormalUnlocked() ? 1 : 0);
        PlayerPrefs.SetInt("HardUnlocked", IsHardUnlocked() ? 1 : 0);

        PlayerPrefs.SetString("SelectedDifficulty", selectedDifficulty);

        // Сохраняем настройки музыки и звуков
        PlayerPrefs.SetInt("MusicOn", musicOn ? 1 : 0);
        PlayerPrefs.SetInt("SoundsOn", soundsOn ? 1 : 0);

        // Сохраняем список ownedSkins как строку, разделённую запятыми
        string skins = string.Join(",", ownedSkins.ToArray());
        PlayerPrefs.SetString("OwnedSkins", skins);

        // Сохраняем выбранный скин
        PlayerPrefs.SetString("SelectedSkin", selectedSkin);

        PlayerPrefs.Save();
        Debug.Log("Данные сохранены.");
    }

    // Метод для загрузки данных
    public void LoadData()
    {
        coins = PlayerPrefs.GetInt("Coins", 0);
        stars = PlayerPrefs.GetInt("Stars", 0);
        gameLevel = PlayerPrefs.GetInt("GameLevel", 1);
        miniGameLevel = PlayerPrefs.GetInt("MiniGameLevel", 1);
        lastDailyRewardClaim = PlayerPrefs.GetString("LastDailyRewardClaim", "0");

        // Загружаем выбранную сложность
        selectedDifficulty = PlayerPrefs.GetString("SelectedDifficulty", "Easy");

        // Загружаем настройки музыки и звуков
        musicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        soundsOn = PlayerPrefs.GetInt("SoundsOn", 1) == 1;

        // Загружаем список ownedSkins
        string skins = PlayerPrefs.GetString("OwnedSkins", "DefaultBall1,DefaultBall2");
        ownedSkins = new List<string>(skins.Split(','));

        // Загружаем выбранный скин
        selectedSkin = PlayerPrefs.GetString("SelectedSkin", "DefaultBall1");

        Debug.Log($"Данные загружены: Coins={coins}, Stars={stars}, SelectedSkin={selectedSkin}, OwnedSkins={skins}");
    }

    // Проверка, разблокирована ли Normal
    public bool IsNormalUnlocked()
    {
        return PlayerPrefs.GetInt("NormalUnlocked", 0) == 1;
    }

    // Проверка, разблокирована ли Hard
    public bool IsHardUnlocked()
    {
        return PlayerPrefs.GetInt("HardUnlocked", 0) == 1;
    }

    // Метод для разблокировки Normal
    public bool UnlockNormal()
    {
        if (stars >= 50 && !IsNormalUnlocked())
        {
            stars -= 50;
            PlayerPrefs.SetInt("NormalUnlocked", 1);
            SaveData();
            Debug.Log("Normal уровень разблокирован.");
            return true;
        }
        Debug.LogWarning("Не удалось разблокировать Normal уровень. Звёзд недостаточно или уже разблокирован.");
        return false;
    }

    // Метод для разблокировки Hard
    public bool UnlockHard()
    {
        if (stars >= 100 && !IsHardUnlocked())
        {
            stars -= 100;
            PlayerPrefs.SetInt("HardUnlocked", 1);
            SaveData();
            Debug.Log("Hard уровень разблокирован.");
            return true;
        }
        Debug.LogWarning("Не удалось разблокировать Hard уровень. Звёзд недостаточно или уже разблокирован.");
        return false;
    }

    // Метод для списания монет
    public bool DeductCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            SaveData();
            Debug.Log($"Списано {amount} монет. Осталось: {coins} монет.");
            return true;
        }
        Debug.LogWarning($"Не удалось списать {amount} монет. Доступно: {coins} монет.");
        return false;
    }

    // Пример метода для обновления монет
    public void AddCoins(int amount)
    {
        coins += amount;
        SaveData();
    }

    // Пример метода для обновления звёзд
    public void AddStars(int amount)
    {
        stars += amount;
        SaveData();
    }

    // Метод для проверки и обновления Daily Rewards
    public bool CanClaimDailyReward()
    {
        if (string.IsNullOrEmpty(lastDailyRewardClaim))
            return true;

        DateTime lastClaim = DateTime.FromBinary(Convert.ToInt64(lastDailyRewardClaim));
        return (DateTime.Now - lastClaim).TotalHours >= 24;
    }

    public void ClaimDailyReward()
    {
        if (CanClaimDailyReward())
        {
            // Добавь логику выдачи награды
            AddCoins(100); // Пример

            // Обнови время последнего получения награды
            lastDailyRewardClaim = DateTime.Now.ToBinary().ToString();
            SaveData();
        }
    }

    // Метод для покупки скина
    public bool PurchaseSkin(string skinName, int price)
    {
        if (ownedSkins.Contains(skinName))
        {
            Debug.LogWarning("Скин уже куплен.");
            return false;
        }

        if (DeductCoins(price))
        {
            ownedSkins.Add(skinName);
            selectedSkin = skinName; // Автоматически выбираем купленный скин
            SaveData();
            OnSkinChanged?.Invoke(); // Уведомляем подписчиков об изменении скина
            Debug.Log($"Скин {skinName} куплен и выбран.");
            return true;
        }

        Debug.LogWarning("Покупка скина не удалась из-за недостатка монет.");
        return false;
    }

    // Метод для выбора скина
    public void SelectSkin(string skinName)
    {
        if (ownedSkins.Contains(skinName))
        {
            selectedSkin = skinName;
            SaveData();
            OnSkinChanged?.Invoke(); // Уведомляем подписчиков об изменении скина
            Debug.Log($"Скин {skinName} выбран.");
        }
        else
        {
            Debug.LogWarning("Скин не куплен.");
        }
    }

    // Метод для получения спрайта скина по имени
    public Sprite GetSkinSprite(string skinName)
    {
        foreach (var skin in allSkins)
        {
            if (skin.skinName == skinName)
                return skin.skinSprite;
        }
        Debug.LogWarning($"Спрайт для скина {skinName} не найден.");
        return null;
    }

    // Метод для сохранения данных при выходе из приложения
    void OnApplicationQuit()
    {
        SaveData();
    }

    // Метод для сброса данных пользователя из инспектора
    [ContextMenu("Reset User Data")]
    public void ResetUserData()
    {
        PlayerPrefs.DeleteAll();
        LoadData();
        Debug.Log("User data has been reset.");
    }

    // Метод для сброса определённых данных пользователя (опционально)
    [ContextMenu("Reset Specific User Data")]
    public void ResetSpecificUserData()
    {
        PlayerPrefs.DeleteKey("Coins");
        PlayerPrefs.DeleteKey("Stars");
        PlayerPrefs.DeleteKey("GameLevel");
        PlayerPrefs.DeleteKey("MiniGameLevel");
        PlayerPrefs.DeleteKey("LastDailyRewardClaim");
        PlayerPrefs.DeleteKey("NormalUnlocked");
        PlayerPrefs.DeleteKey("HardUnlocked");
        PlayerPrefs.DeleteKey("SelectedDifficulty");
        PlayerPrefs.DeleteKey("MusicOn");
        PlayerPrefs.DeleteKey("SoundsOn");
        PlayerPrefs.DeleteKey("OwnedSkins");
        PlayerPrefs.DeleteKey("SelectedSkin");
        LoadData();
        Debug.Log("Specific user data has been reset.");
    }
}
