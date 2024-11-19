using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button musicButton;
    public Button soundsButton;

    [Header("Sprites")]
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;
    public Sprite soundsOnSprite;
    public Sprite soundsOffSprite;

    [Header("Button Images")]
    public Image musicButtonImage;
    public Image soundsButtonImage;

    void Start()
    {
        // Проверка назначены ли все ссылки
        if (musicButton == null || soundsButton == null ||
            musicButtonImage == null || soundsButtonImage == null)
        {
            Debug.LogError("Не все ссылки назначены в SettingsPanelManager.");
            return;
        }

        // Установить начальные спрайты на основе текущих настроек
        UpdateMusicButtonSprite();
        UpdateSoundsButtonSprite();

        // Добавляем слушателей для кнопок
        musicButton.onClick.AddListener(ToggleMusic);
        soundsButton.onClick.AddListener(ToggleSounds);
    }

    // Метод для переключения музыки
    void ToggleMusic()
    {
        if (DataManager.Instance == null || AudioManager.Instance == null)
        {
            Debug.LogError("DataManager.Instance или AudioManager.Instance не инициализированы.");
            return;
        }

        // Переключаем состояние
        DataManager.Instance.musicOn = !DataManager.Instance.musicOn;
        DataManager.Instance.SaveData();

        // Обновляем AudioManager
        AudioManager.Instance.ToggleMusic(DataManager.Instance.musicOn);

        // Обновляем спрайт кнопки
        UpdateMusicButtonSprite();

        // Воспроизводим звук нажатия кнопки
        AudioManager.Instance.PlayButtonClick();
    }

    // Метод для переключения звуков
    void ToggleSounds()
    {
        if (DataManager.Instance == null || AudioManager.Instance == null)
        {
            Debug.LogError("DataManager.Instance или AudioManager.Instance не инициализированы.");
            return;
        }

        // Переключаем состояние
        DataManager.Instance.soundsOn = !DataManager.Instance.soundsOn;
        DataManager.Instance.SaveData();

        // Обновляем AudioManager
        AudioManager.Instance.ToggleSounds(DataManager.Instance.soundsOn);

        // Обновляем спрайт кнопки
        UpdateSoundsButtonSprite();

        // Воспроизводим звук нажатия кнопки
        AudioManager.Instance.PlayButtonClick();
    }

    // Метод для обновления спрайта кнопки музыки
    void UpdateMusicButtonSprite()
    {
        if (DataManager.Instance.musicOn)
        {
            musicButtonImage.sprite = musicOnSprite;
        }
        else
        {
            musicButtonImage.sprite = musicOffSprite;
        }
    }

    // Метод для обновления спрайта кнопки звуков
    void UpdateSoundsButtonSprite()
    {
        if (DataManager.Instance.soundsOn)
        {
            soundsButtonImage.sprite = soundsOnSprite;
        }
        else
        {
            soundsButtonImage.sprite = soundsOffSprite;
        }
    }

    void OnDestroy()
    {
        // Удаляем слушателей при уничтожении объекта
        if (musicButton != null)
            musicButton.onClick.RemoveListener(ToggleMusic);
        if (soundsButton != null)
            soundsButton.onClick.RemoveListener(ToggleSounds);
    }
}
