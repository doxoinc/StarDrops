using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music")]
    public AudioSource musicSource;
    public AudioClip backgroundMusic;

    [Header("Sound Effects")]
    public AudioSource sfxSource;
    public AudioClip buttonClickSound;

    [Header("Additional Sound Effects")]
    public AudioClip purchaseSound; // Новый звук покупки

    void Awake()
    {
        // Реализация паттерна Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("Duplicate AudioManager destroyed.");
        }
    }

    void InitializeAudio()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
        }

        // Настройка фоновой музыки
        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning("Background music not assigned in AudioManager.");
        }

        // Настройка звука кнопки
        if (buttonClickSound == null)
        {
            Debug.LogWarning("Button click sound not assigned in AudioManager.");
        }

        // Настройка звука покупки
        if (purchaseSound == null)
        {
            Debug.LogWarning("Purchase sound not assigned in AudioManager.");
        }

        // Установить начальное состояние музыки и звуков
        UpdateMusicState();
        UpdateSoundsState();
    }

    // Метод для воспроизведения звука кнопки
    public void PlayButtonClick()
    {
        if (DataManager.Instance != null && DataManager.Instance.soundsOn)
        {
            if (buttonClickSound != null)
            {
                sfxSource.PlayOneShot(buttonClickSound);
            }
            else
            {
                Debug.LogWarning("Button click sound not assigned.");
            }
        }
    }

    // Новый метод для воспроизведения звука покупки
    public void PlayPurchaseSound()
    {
        if (DataManager.Instance != null && DataManager.Instance.soundsOn)
        {
            if (purchaseSound != null)
            {
                sfxSource.PlayOneShot(purchaseSound);
            }
            else
            {
                Debug.LogWarning("Purchase sound not assigned.");
            }
        }
    }

    // Метод для обновления состояния музыки
    public void ToggleMusic(bool isOn)
    {
        if (isOn)
        {
            musicSource.Play();
        }
        else
        {
            musicSource.Pause();
        }
    }

    // Метод для обновления состояния звуков
    public void ToggleSounds(bool isOn)
    {
        // Здесь можно добавить дополнительные действия при включении/выключении звуков
        // Например, регулировка громкости звуковых эффектов
        sfxSource.mute = !isOn;
    }

    // Метод для обновления состояния музыки при запуске игры
    public void UpdateMusicState()
    {
        if (DataManager.Instance != null)
        {
            ToggleMusic(DataManager.Instance.musicOn);
        }
    }

    // Метод для обновления состояния звуков при запуске игры
    public void UpdateSoundsState()
    {
        if (DataManager.Instance != null)
        {
            ToggleSounds(DataManager.Instance.soundsOn);
        }
    }
}
