using UnityEngine;

public class BallSkinManager : MonoBehaviour
{
    public SpriteRenderer spriteRenderer; // Привяжите через Inspector

    // Желаемый размер шарика
    public Vector3 desiredScale = new Vector3(0.5f, 0.5f, 0.5f);

    void Start()
    {
        ApplySelectedSkin();
        SetDesiredScale();
    }

    void OnEnable()
    {
        // Подписываемся на событие изменения скина
        if (DataManager.Instance != null)
            DataManager.Instance.OnSkinChanged += ApplySelectedSkin;
    }

    void OnDisable()
    {
        // Отписываемся от события
        if (DataManager.Instance != null)
            DataManager.Instance.OnSkinChanged -= ApplySelectedSkin;
    }

    public void ApplySelectedSkin()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("SpriteRenderer не найден на Ball.");
                return;
            }
        }

        string selectedSkin = DataManager.Instance.selectedSkin;
        Sprite selectedSprite = DataManager.Instance.GetSkinSprite(selectedSkin);

        // Если спрайт не найден, используем базовый спрайт
        if (selectedSprite == null)
        {
            Debug.LogWarning($"Спрайт для скина {selectedSkin} не найден. Используем базовый спрайт.");
            return;
        }

        // Назначить спрайт
        spriteRenderer.sprite = selectedSprite;
        Debug.Log($"BallSkinManager: Назначен скин {selectedSkin}");

        // Установить желаемый размер после назначения скина
        SetDesiredScale();
    }

    // Метод для установки желаемого размера шарика
    public void SetDesiredScale()
    {
        // Устанавливаем локальный масштаб
        transform.localScale = desiredScale;
        Debug.Log($"BallSkinManager: Установлен размер шарика на {desiredScale}");
    }
}
