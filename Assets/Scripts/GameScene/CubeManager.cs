using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CubeManager : MonoBehaviour
{
    public static CubeManager Instance;

    [Header("Grid Settings")]
    public Grid grid; // Ссылка на Grid
    public GameObject cubePrefab; // Префаб кубика

    public int gridWidth = 5; // Ширина Grid (настраивается в Inspector)
    public int gridHeight = 5; // Высота Grid (настраивается в Inspector)

    private Cube[,] cubeGrid; // 2D массив для управления сеткой
    private List<Cube> cubes = new List<Cube>();

    // Переменные для перетаскивания
    private Cube selectedCube;
    private Vector3 originalPosition;
    private bool isDragging = false;
    private GridPosition originalGridPos; // Для хранения исходной позиции в сетке

    // Минимальное смещение для обмена
    private float minDragDistance = 0.3f;

    // Половина размеров ячейки
    private Vector3 halfCellSize;

    // Смещение для центрирования сетки
    private Vector3 gridOffset;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        cubeGrid = new Cube[gridWidth, gridHeight];
        halfCellSize = new Vector3(grid.cellSize.x / 2f, grid.cellSize.y / 2f, 0);

        // Вычисляем смещение для центрирования сетки
        gridOffset = new Vector3((gridWidth * grid.cellSize.x) / 2f,
                                 (gridHeight * grid.cellSize.y) / 2f, 0);

        SpawnInitialCubes();

        // Спавн шарика в начальной позиции
        if (BallSpawner.Instance != null)
        {
            BallSpawner.Instance.SpawnBall();
            Debug.Log("Шарик спавнен в начальной позиции.");
        }
        else
        {
            Debug.LogError("BallSpawner.Instance равен null. Убедитесь, что BallSpawner присутствует на сцене и инициализирован.");
        }
    }

    // Метод для спауна начальных кубиков без создания совпадений
    void SpawnInitialCubes()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                Vector3 worldPosition = grid.CellToWorld(cellPosition) - gridOffset + halfCellSize; // Центрируем сетку

                // Выбираем допустимый цвет, избегая создания начальных совпадений
                Cube.CubeColor color = GetValidRandomColor(x, y);

                // Спавн кубика
                GameObject cubeObj = Instantiate(cubePrefab, worldPosition, Quaternion.identity, grid.transform);
                Cube cube = cubeObj.GetComponent<Cube>();
                cube.SetColor(color);

                // Назначаем кубик в сетку и список
                cubeGrid[x, y] = cube;
                cubes.Add(cube);

                // Визуализация границ ячейки (опционально для отладки)
                // ... (код визуализации, если нужен)
            }
        }

        Debug.Log("Начальные кубики спавнены без совпадений.");
    }

    // Метод для выбора допустимого случайного цвета, избегая начальных совпадений
    Cube.CubeColor GetValidRandomColor(int x, int y)
    {
        List<Cube.CubeColor> availableColors = new List<Cube.CubeColor>((Cube.CubeColor[])Enum.GetValues(typeof(Cube.CubeColor)));

        // Проверка горизонтальных совпадений (два предыдущих кубика слева)
        if (x >= 2)
        {
            Cube.CubeColor left1 = cubeGrid[x - 1, y].cubeColor;
            Cube.CubeColor left2 = cubeGrid[x - 2, y].cubeColor;
            if (left1 == left2)
            {
                availableColors.Remove(left1);
            }
        }

        // Проверка вертикальных совпадений (два предыдущих кубика снизу)
        if (y >= 2)
        {
            Cube.CubeColor down1 = cubeGrid[x, y - 1].cubeColor;
            Cube.CubeColor down2 = cubeGrid[x, y - 2].cubeColor;
            if (down1 == down2)
            {
                availableColors.Remove(down1);
            }
        }

        // Выбор случайного цвета из доступных
        if (availableColors.Count > 0)
        {
            return availableColors[UnityEngine.Random.Range(0, availableColors.Count)];
        }
        else
        {
            // В редких случаях, если нет доступных цветов, выбираем любой
            Array colors = Enum.GetValues(typeof(Cube.CubeColor));
            return (Cube.CubeColor)colors.GetValue(UnityEngine.Random.Range(0, colors.Length));
        }
    }

    void Update()
    {
        HandleInput();
    }

    // Обработка ввода для перетаскивания кубиков
    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Преобразование координат мыши в мировые
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = -Camera.main.transform.position.z; // Расстояние от камеры до плоскости кубиков
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

            Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

            Collider2D hitCollider = Physics2D.OverlapPoint(mousePos2D);
            if (hitCollider != null)
            {
                Cube cube = hitCollider.GetComponent<Cube>();
                if (cube != null)
                {
                    selectedCube = cube;
                    originalPosition = selectedCube.transform.position;
                    isDragging = true;

                    originalGridPos = GetGridPosition(selectedCube); // Сохраняем исходную позицию в сетке
                    Debug.Log($"Выбран кубик на позиции ({originalGridPos.x}, {originalGridPos.y})");
                }
            }
        }

        if (Input.GetMouseButton(0) && isDragging && selectedCube != null)
        {
            // Преобразование координат мыши в мировые
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = -Camera.main.transform.position.z; // Расстояние от камеры до плоскости кубиков
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

            selectedCube.transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, originalPosition.z);
        }

        if (Input.GetMouseButtonUp(0) && isDragging && selectedCube != null)
        {
            isDragging = false;

            // Преобразование координат мыши в мировые
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = -Camera.main.transform.position.z; // Расстояние от камеры до плоскости кубиков
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

            // Учитываем смещение при вычислении целевой ячейки
            Vector3Int targetCellPosition = grid.WorldToCell(mouseWorldPos + gridOffset);
            Vector3 targetSnapPosition = grid.CellToWorld(targetCellPosition) - gridOffset + halfCellSize; // Центр ячейки

            // Используем сохранённую исходную позицию
            int originalX = originalGridPos.x;
            int originalY = originalGridPos.y;

            // Получаем координаты целевой ячейки
            GridPosition targetGridPos = new GridPosition(targetCellPosition.x, targetCellPosition.y);

            // Дополнительное логирование
            Debug.Log($"Позиция мыши в мире: {mouseWorldPos}");
            Debug.Log($"Целевая позиция ячейки: ({targetCellPosition.x}, {targetCellPosition.y})");

            // Логирование исходной и целевой позиций
            Debug.Log($"Исходная позиция в сетке: ({originalGridPos.x}, {originalGridPos.y})");
            Debug.Log($"Целевая позиция в сетке: ({targetGridPos.x}, {targetGridPos.y})");

            // Проверка, находится ли целевая ячейка внутри сетки
            if (targetCellPosition.x < 0 || targetCellPosition.x >= gridWidth ||
                targetCellPosition.y < 0 || targetCellPosition.y >= gridHeight)
            {
                // Вне сетки, возвращаемся на исходную позицию
                StartCoroutine(MoveCube(selectedCube, originalPosition, 0.2f));
                selectedCube = null;
                Debug.Log("Целевая позиция вне сетки. Возвращаем кубик на исходную позицию.");
                return;
            }

            // Проверка, соседняя ли целевая ячейка
            bool isAdjacent = IsAdjacent(originalGridPos, targetGridPos);
            Debug.Log($"Соседняя ли ячейка: {isAdjacent}");

            // Вычисление расстояния между исходной и конечной позицией кубика
            float distance = Vector3.Distance(originalPosition, selectedCube.transform.position);
            Debug.Log($"Расстояние перемещения: {distance}");

            if (!isAdjacent)
            {
                // Не соседняя ячейка, возвращаем кубик
                StartCoroutine(MoveCube(selectedCube, originalPosition, 0.2f));
                selectedCube = null;
                Debug.Log("Целевая ячейка не соседняя. Возвращаем кубик на исходную позицию.");
                return;
            }

            // Получаем кубик в целевой ячейке
            Cube existingCube = GetCubeAtCell(targetGridPos);
            Debug.Log($"Кубик в целевой ячейке: {(existingCube != null ? existingCube.cubeColor.ToString() : "Отсутствует")}");

            if (existingCube != null && existingCube != selectedCube)
            {
                // Целевая ячейка занята другим кубиком, выполняем обмен

                // Обмен в сетке
                cubeGrid[originalX, originalY] = existingCube;
                cubeGrid[targetGridPos.x, targetGridPos.y] = selectedCube;

                // Обмен позиций с использованием корутин
                StartCoroutine(MoveCube(existingCube, originalPosition, 0.2f));
                StartCoroutine(MoveCube(selectedCube, targetSnapPosition, 0.2f));

                // Проверка на наличие совпадений после обмена
                StartCoroutine(CheckMatchAfterSwap(existingCube, selectedCube, originalGridPos, targetGridPos));
            }
            else
            {
                // Целевая ячейка свободна, просто перемещаем кубик

                // Обновляем сетку
                cubeGrid[originalX, originalY] = null;
                cubeGrid[targetGridPos.x, targetGridPos.y] = selectedCube;

                // Перемещаем кубик с использованием корутины
                StartCoroutine(MoveCube(selectedCube, targetSnapPosition, 0.2f));

                // Проверка на наличие совпадений
                StartCoroutine(CheckMatchAfterMove(selectedCube, originalGridPos, targetGridPos));
            }

            selectedCube = null;
        } // Закрытие метода HandleInput

    } // Закрытие класса CubeManager

    #region Корутинные Методы

    // Метод для плавного перемещения кубика
    IEnumerator MoveCube(Cube cube, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = cube.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            cube.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cube.transform.position = targetPosition;
    }

    // Метод для проверки совпадений после обмена
    IEnumerator CheckMatchAfterSwap(Cube existingCube, Cube selectedCube, GridPosition originalGridPos, GridPosition targetGridPos)
    {
        // Ждём окончания анимации обмена
        yield return new WaitForSeconds(0.25f);

        List<Cube> matchedCubes = CheckForMatches();

        if (matchedCubes.Count > 0)
        {
            RemoveMatchedCubes(matchedCubes);
        }
        else
        {
            // Совпадений нет, отменяем обмен

            Debug.Log("Совпадений не найдено после обмена. Отмена обмена.");

            // Обмен обратно в сетке
            cubeGrid[originalGridPos.x, originalGridPos.y] = selectedCube;
            cubeGrid[targetGridPos.x, targetGridPos.y] = existingCube;

            // Обмен позиций обратно с использованием корутин
            StartCoroutine(MoveCube(existingCube, GetWorldPosition(originalGridPos), 0.2f));
            StartCoroutine(MoveCube(selectedCube, GetWorldPosition(targetGridPos), 0.2f));
        }
    }

    // Метод для проверки совпадений после перемещения
    IEnumerator CheckMatchAfterMove(Cube cube, GridPosition originalGridPos, GridPosition targetGridPos)
    {
        // Ждём окончания анимации перемещения
        yield return new WaitForSeconds(0.25f);

        List<Cube> matchedCubes = CheckForMatches();

        if (matchedCubes.Count > 0)
        {
            RemoveMatchedCubes(matchedCubes);
        }
        else
        {
            // Совпадений нет, возвращаем кубик на исходную позицию

            Debug.Log("Совпадений не найдено после перемещения. Возвращаем кубик на исходную позицию.");

            // Обновляем сетку
            cubeGrid[targetGridPos.x, targetGridPos.y] = null;
            cubeGrid[originalGridPos.x, originalGridPos.y] = cube;

            // Перемещаем кубик обратно с использованием корутины
            StartCoroutine(MoveCube(cube, GetWorldPosition(originalGridPos), 0.2f));
        }
    }

    #endregion

    #region Вспомогательные Методы

    // Метод для получения координат кубика в сетке
    GridPosition GetGridPosition(Cube cube)
    {
        Vector3 worldPos = cube.transform.position + gridOffset; // Учёт смещения
        Vector3Int cellPos = grid.WorldToCell(worldPos);
        Debug.Log($"Позиция кубика в мире: {cube.transform.position}, позиция ячейки: ({cellPos.x}, {cellPos.y})");
        return new GridPosition(cellPos.x, cellPos.y);
    }

    // Метод для проверки соседства двух ячеек
    bool IsAdjacent(GridPosition pos1, GridPosition pos2)
    {
        int dx = Mathf.Abs(pos1.x - pos2.x);
        int dy = Mathf.Abs(pos1.y - pos2.y);
        return (dx + dy) == 1;
    }

    // Метод для получения мировой позиции ячейки
    public Vector3 GetWorldPosition(GridPosition gridPos)
    {
        if (gridPos.x < 0 || gridPos.x >= gridWidth || gridPos.y < 0 || gridPos.y >= gridHeight)
        {
            Debug.LogError($"GetWorldPosition: GridPosition ({gridPos.x}, {gridPos.y}) находится вне границ.");
            return Vector3.zero;
        }
        Vector3Int cellPos = gridPos.ToVector3Int();
        return grid.CellToWorld(cellPos) - gridOffset + halfCellSize;
    }

    // Метод для установки isTrigger для всех кубиков
    public void SetAllCubesIsTrigger(bool value)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (cubeGrid[x, y] != null)
                {
                    BoxCollider2D collider = cubeGrid[x, y].GetComponent<BoxCollider2D>();
                    if (collider != null)
                    {
                        collider.isTrigger = value;
                        Debug.Log($"Box at ({x}, {y}) isTrigger set to {value}");
                    }
                    else
                    {
                        Debug.LogWarning($"Box at ({x}, {y}) does not have BoxCollider2D");
                    }
                }
            }
        }
    }

    // Метод для поиска самой нижней свободной ячейки
    public GridPosition FindLowestFreeCell()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (cubeGrid[x, y] == null)
                {
                    return new GridPosition(x, y);
                }
            }
        }
        return new GridPosition(-1, -1); // Индикатор отсутствия свободных ячеек
    }

    // Метод для проверки наличия совпадений в сетке и возврат списка совпавших кубиков
    public List<Cube> CheckForMatches()
    {
        List<Cube> matchedCubes = new List<Cube>();

        // Горизонтальная проверка
        for (int y = 0; y < gridHeight; y++)
        {
            int count = 1;
            for (int x = 1; x < gridWidth; x++)
            {
                if (cubeGrid[x, y] != null && cubeGrid[x - 1, y] != null &&
                    cubeGrid[x, y].cubeColor == cubeGrid[x - 1, y].cubeColor)
                {
                    count++;
                }
                else
                {
                    if (count >= 3)
                    {
                        Debug.Log($"Горизонтальное совпадение в строке {y}, начиная с колонки {x - count}");
                        for (int i = x - count; i < x; i++)
                        {
                            if (!matchedCubes.Contains(cubeGrid[i, y]))
                                matchedCubes.Add(cubeGrid[i, y]);
                        }
                    }
                    count = 1;
                }
            }

            if (count >= 3)
            {
                Debug.Log($"Горизонтальное совпадение в строке {y}, начиная с колонки {gridWidth - count}");
                for (int i = gridWidth - count; i < gridWidth; i++)
                {
                    if (!matchedCubes.Contains(cubeGrid[i, y]))
                        matchedCubes.Add(cubeGrid[i, y]);
                }
            }
        }

        // Вертикальная проверка
        for (int x = 0; x < gridWidth; x++)
        {
            int count = 1;
            for (int y = 1; y < gridHeight; y++)
            {
                if (cubeGrid[x, y] != null && cubeGrid[x, y - 1] != null &&
                    cubeGrid[x, y].cubeColor == cubeGrid[x, y - 1].cubeColor)
                {
                    count++;
                }
                else
                {
                    if (count >= 3)
                    {
                        Debug.Log($"Вертикальное совпадение в колонке {x}, начиная с строки {y - count}");
                        for (int i = y - count; i < y; i++)
                        {
                            if (!matchedCubes.Contains(cubeGrid[x, i]))
                                matchedCubes.Add(cubeGrid[x, i]);
                        }
                    }
                    count = 1;
                }
            }

            if (count >= 3)
            {
                Debug.Log($"Вертикальное совпадение в колонке {x}, начиная с строки {gridHeight - count}");
                for (int i = gridHeight - count; i < gridHeight; i++)
                {
                    if (!matchedCubes.Contains(cubeGrid[x, i]))
                        matchedCubes.Add(cubeGrid[x, i]);
                }
            }
        }

        return matchedCubes;
    }

    // Удаление совпавших кубиков
    public void RemoveMatchedCubes(List<Cube> matchedCubes)
    {
        if (matchedCubes.Count == 0)
            return;

        Debug.Log($"Удаление {matchedCubes.Count} совпавших кубиков.");

        foreach (Cube matched in matchedCubes)
        {
            GridPosition gridPos = GetGridPosition(matched);
            cubeGrid[gridPos.x, gridPos.y] = null;
            cubes.Remove(matched);
            Debug.Log($"Удаление кубика на позиции ({gridPos.x}, {gridPos.y}) с цветом {matched.cubeColor}.");
            Destroy(matched.gameObject);
        }

        // Включаем isTrigger для всех кубиков, чтобы шарик мог падать вниз
        SetAllCubesIsTrigger(true);
        Debug.Log("All Boxes set to isTrigger=true to allow FinalBall to pass through.");

        // Уведомляем GameManager о новом совпадении только если игра не завершена
        if (GameManager.Instance != null && !GameManager.Instance.HasFinished())
        {
            GameManager.Instance.IncrementMatchCount();
        }
        else
        {
            Debug.Log("Игра уже завершена или GameManager.Instance равен null.");
        }
    }

    // Получение кубика по позиции ячейки
    public Cube GetCubeAtCell(GridPosition cellPos)
    {
        if (cellPos.x < 0 || cellPos.x >= gridWidth || cellPos.y < 0 || cellPos.y >= gridHeight)
            return null;

        return cubeGrid[cellPos.x, cellPos.y];
    }

    #endregion
}
