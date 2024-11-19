using UnityEngine;

public class Box : MonoBehaviour
{
    public enum EffectType { Add, Multiply, Subtract, Divide }
    public EffectType effectType;
    public float effectValue = 10f; // Значение эффекта
    public int scoreValue = 10; // Очки за попадание

    private bool hasFinalBallPassed = false; // Флаг, чтобы эффект применялся только один раз

    public void TriggerEffect()
    {
        GameManager.Instance.ApplyEffect(effectType, effectValue);
    }

    // Обработка столкновений с FinalBall
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("FinalBall") && !hasFinalBallPassed)
        {
            hasFinalBallPassed = true;
            TriggerEffect();

            // Устанавливаем isTrigger=true для этого Box
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
                Debug.Log($"Box at {transform.position} set to isTrigger=true");
            }
            else
            {
                Debug.LogWarning($"Box at {transform.position} does not have BoxCollider2D");
            }
        }
    }

    // Если вы используете триггеры вместо столкновений
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("FinalBall") && !hasFinalBallPassed)
        {
            hasFinalBallPassed = true;
            TriggerEffect();

            // Устанавливаем isTrigger=true для этого Box
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
                Debug.Log($"Box at {transform.position} set to isTrigger=true");
            }
            else
            {
                Debug.LogWarning($"Box at {transform.position} does not have BoxCollider2D");
            }
        }
    }
}
