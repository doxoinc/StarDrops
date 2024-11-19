using UnityEngine;

public class Ball : MonoBehaviour
{
    public float speed = 5f; // Начальная скорость мяча
    private Rigidbody2D rb;
    private bool hasCollidedWithBox = false;
    private bool isFinalBall = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Запускаем мяч вниз с заданной скоростью
        rb.velocity = Vector2.down * speed;

        // Проверяем, является ли этот шарик финальным
        if (gameObject.CompareTag("FinalBall"))
        {
            isFinalBall = true;
        }
    }

    // Обработка столкновений с коллайдерами, не являющимися триггерами
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Boundary"))
        {
            // Отражение шарика при столкновении с препятствиями или границами
            Vector2 normal = collision.contacts[0].normal;
            Vector2 newDirection = Vector2.Reflect(rb.velocity.normalized, normal);
            rb.velocity = newDirection * speed;
        }

        // Обработка столкновения обычного шарика с Box
        if (!isFinalBall && collision.gameObject.CompareTag("Box"))
        {
            if (!hasCollidedWithBox)
            {
                hasCollidedWithBox = true;

                Box box = collision.gameObject.GetComponent<Box>();
                if (box != null)
                {
                    box.TriggerEffect();
                }

                // Добавление очков игроку
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.UpdateBalance(box.scoreValue);
                }
                else
                {
                    Debug.LogError("GameManager.Instance равен null. Убедитесь, что GameManager присутствует на сцене.");
                }

                // Уничтожаем мяч после взаимодействия
                Destroy(gameObject);

                // Устанавливаем isTrigger обратно на все кубики
                if (CubeManager.Instance != null)
                {
                    CubeManager.Instance.SetAllCubesIsTrigger(false);
                }
                else
                {
                    Debug.LogError("CubeManager.Instance равен null. Убедитесь, что CubeManager присутствует на сцене и инициализирован.");
                }

                // Находим самую нижнюю свободную ячейку
                if (CubeManager.Instance != null && BallSpawner.Instance != null)
                {
                    GridPosition lowestFreeCell = CubeManager.Instance.FindLowestFreeCell();
                    if (lowestFreeCell.x != -1 && lowestFreeCell.y != -1)
                    {
                        Vector3 spawnPosition = CubeManager.Instance.GetWorldPosition(lowestFreeCell);

                        // Проверяем, что текущий шарик не является FinalBall перед спавном нового
                        if (!isFinalBall)
                        {
                            BallSpawner.Instance.SpawnBallAt(spawnPosition);
                            Debug.Log($"Спавн нового шарика на позиции {spawnPosition}.");
                        }
                        else
                        {
                            Debug.Log("FinalBall не будет спавнить новый шарик.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Нет свободных ячеек для спавна нового шарика.");
                    }
                }
            }
        }
    }

    // Обработка триггерных столкновений (используется для FinalBall)
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isFinalBall && collision.gameObject.CompareTag("Box"))
        {
            Box box = collision.gameObject.GetComponent<Box>();
            if (box != null)
            {
                box.TriggerEffect();
            }

            // Добавление очков игроку
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdateBalance(box.scoreValue);
            }
            else
            {
                Debug.LogError("GameManager.Instance равен null. Убедитесь, что GameManager присутствует на сцене.");
            }

            // Устанавливаем isTrigger на все кубики, чтобы шарик мог пройти сквозь
            if (CubeManager.Instance != null)
            {
                CubeManager.Instance.SetAllCubesIsTrigger(true);
                Debug.Log("All Boxes set to isTrigger=true to allow FinalBall to pass through.");
            }
            else
            {
                Debug.LogError("CubeManager.Instance равен null. Убедитесь, что CubeManager присутствует на сцене и инициализирован.");
            }

            // Финальный шарик не уничтожается и продолжает движение к FinishLine
        }

        if (collision.gameObject.CompareTag("Finish"))
        {
            // Проверка, что это FinalBall
            if (isFinalBall)
            {
                Debug.Log("FinalBall достиг FinishLine.");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TriggerGameEnd(); // Победа
                }
                else
                {
                    Debug.LogError("GameManager.Instance равен null. Убедитесь, что GameManager присутствует на сцене.");
                }
                Destroy(gameObject);
            }
        }
    }

    // Метод для случайного изменения траектории шарика (не используется, но может быть полезен)
    void ChangeTrajectoryRandomly()
    {
        float angle = UnityEngine.Random.Range(-60f, 60f); // Угол поворота
        float rad = angle * Mathf.Deg2Rad;
        Vector2 newDirection = new Vector2(Mathf.Sin(rad), -Mathf.Cos(rad)).normalized;
        rb.velocity = newDirection * speed;
    }
}
