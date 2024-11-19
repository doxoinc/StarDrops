using UnityEngine;

public class Cube : MonoBehaviour
{
    public enum CubeColor { Red, Green, Blue, Yellow }
    public CubeColor cubeColor;
    private SpriteRenderer spriteRenderer;

    [Header("Sprites")]
    public Sprite redSprite;
    public Sprite greenSprite;
    public Sprite blueSprite;
    public Sprite yellowSprite;

    [Header("Selection")]
    public Color selectedColor = Color.white;
    private Color originalColor;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        originalColor = spriteRenderer.color;
    }

    // Метод для установки цвета кубика
    public void SetColor(CubeColor color)
    {
        cubeColor = color;
        UpdateColor();
    }

    // Обновление спрайта кубика в зависимости от его цвета
    void UpdateColor()
    {
        switch (cubeColor)
        {
            case CubeColor.Red:
                spriteRenderer.sprite = redSprite;
                break;
            case CubeColor.Green:
                spriteRenderer.sprite = greenSprite;
                break;
            case CubeColor.Blue:
                spriteRenderer.sprite = blueSprite;
                break;
            case CubeColor.Yellow:
                spriteRenderer.sprite = yellowSprite;
                break;
        }
    }

    // Метод вызывается при нажатии на кубик
    void OnMouseDown()
    {
        spriteRenderer.color = selectedColor;
    }

    // Метод вызывается при отпускании кубика
    void OnMouseUp()
    {
        spriteRenderer.color = originalColor;
    }
}
