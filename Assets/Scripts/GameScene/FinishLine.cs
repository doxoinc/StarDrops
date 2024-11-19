using UnityEngine;

public class FinishLine : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("FinalBall"))
        {
            Debug.Log("Финальный шарик достиг полоски финиша.");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameEnd();
            }
            else
            {
                Debug.LogError("GameManager.Instance равен null.");
            }
        }
    }
}
