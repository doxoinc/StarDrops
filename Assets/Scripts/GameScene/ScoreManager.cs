using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public UIManager uiManager;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Метод для добавления очков
    public void AddScore(int amount)
    {
        // Предполагается, что очки добавляются к текущему балансу
        GameManager.Instance.UpdateBalance(amount);
    }
}
