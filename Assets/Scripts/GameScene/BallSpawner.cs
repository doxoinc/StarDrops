using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    public static BallSpawner Instance;

    public GameObject ballPrefab; // Префаб обычного шарика
    public GameObject finalBallPrefab; // Префаб финального шарика
    public Transform spawnPoint; // Точка спавна шарика (начальная позиция)

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Метод для спавна обычного шарика в начальной позиции
    public void SpawnBall()
    {
        Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
    }

    // Метод для спавна шарика в заданной позиции
    public void SpawnBallAt(Vector3 position)
    {
        Instantiate(ballPrefab, position, Quaternion.identity);
    }

    // Метод для спавна финального шарика
    public void SpawnFinalBall()
    {
        if (finalBallPrefab != null && spawnPoint != null)
        {
            GameObject finalBall = Instantiate(finalBallPrefab, spawnPoint.position, Quaternion.identity);
            finalBall.tag = "FinalBall"; // Убедитесь, что тег "FinalBall" создан и назначен
        }
        else
        {
            Debug.LogError("FinalBallPrefab или spawnPoint не назначены в BallSpawner.");
        }
    }

    // Новый метод для замены текущего шарика на финальный шарик
    public void ReplaceBallWithFinalBall(Ball currentBall)
    {
        if (finalBallPrefab != null && currentBall != null)
        {
            Vector3 position = currentBall.transform.position;
            Quaternion rotation = currentBall.transform.rotation;

            // Удаляем текущий шарик
            Destroy(currentBall.gameObject);

            // Спавним финальный шарик на той же позиции
            GameObject finalBall = Instantiate(finalBallPrefab, position, rotation);
            finalBall.tag = "FinalBall"; // Назначаем тег "FinalBall"

            Debug.Log($"Заменён обычный шарик на FinalBall в позиции {position}.");
        }
        else
        {
            Debug.LogError("FinalBallPrefab не назначен или currentBall равен null.");
        }
    }
}
