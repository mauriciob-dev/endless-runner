using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Genera obstáculos aleatorios a intervalos regulares.
/// La dificultad aumenta con el tiempo.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Prefabs de Obstáculos")]
    [Tooltip("Lista de prefabs de obstáculos disponibles")]
    [SerializeField] private GameObject[] obstaclePrefabs;

    [Header("Configuración de Spawn")]
    [Tooltip("Distancia mínima entre obstáculos")]
    [SerializeField] private float minSpawnDistance = 6f;

    [Tooltip("Distancia máxima entre obstáculos")]
    [SerializeField] private float maxSpawnDistance = 10f;

    [Tooltip("Distancia adelante del jugador donde aparecen obstáculos")]
    [SerializeField] private float spawnAheadDistance = 15f;

    [Tooltip("Altura Y donde se generan los obstáculos")]
    [SerializeField] private float spawnHeight = 0f;

    [Header("Dificultad Progresiva")]
    [Tooltip("¿Aumentar dificultad con el tiempo?")]
    [SerializeField] private bool increaseDifficulty = true;

    [Tooltip("Cada cuántos segundos reduce la distancia entre obstáculos")]
    [SerializeField] private float difficultyIncreaseInterval = 15f;

    [Tooltip("Cuánto reduce la distancia cada intervalo")]
    [SerializeField] private float distanceDecreaseAmount = 0.3f;

    [Tooltip("Distancia mínima absoluta (no puede bajar de esto)")]
    [SerializeField] private float absoluteMinDistance = 2f;

    [Header("Referencias")]
    [Tooltip("Transform del jugador (se busca automáticamente si está vacío)")]
    [SerializeField] private Transform playerTransform;

    [Header("Control del Spawner")]
    [Tooltip("¿Iniciar generación automáticamente?")]
    [SerializeField] private bool autoStart = true;

    [Tooltip("Delay antes de generar el primer obstáculo (segundos)")]
    [SerializeField] private float initialDelay = 2f;

    // Variables privadas
    private float nextSpawnX; // Posición X del próximo obstáculo
    private bool isSpawning = false;
    private float gameStartTime;
    private float lastDifficultyIncreaseTime;
    private float currentMinDistance;
    private float currentMaxDistance;

    // Lista de obstáculos activos (para debugging)
    private List<GameObject> activeObstacles = new List<GameObject>();

    void Start()
    {
        // Buscar jugador automáticamente
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("ObstacleSpawner: No se encontró el Player!");
                return;
            }
        }

        // Verificar que hay prefabs asignados
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            Debug.LogError("ObstacleSpawner: No hay prefabs de obstáculos asignados!");
            return;
        }

        // Inicializar valores
        currentMinDistance = minSpawnDistance;
        currentMaxDistance = maxSpawnDistance;
        gameStartTime = Time.time;
        lastDifficultyIncreaseTime = 0f;

        // Calcular posición inicial de spawn
        nextSpawnX = playerTransform.position.x + spawnAheadDistance;

        // Iniciar generación
        if (autoStart)
        {
            Invoke(nameof(StartSpawning), initialDelay);
        }
    }

    void Update()
    {
        if (!isSpawning || playerTransform == null) return;

        // Generar obstáculos cuando sea necesario
        if (playerTransform.position.x + spawnAheadDistance >= nextSpawnX)
        {
            SpawnObstacle();
        }

        // Aumentar dificultad progresivamente
        if (increaseDifficulty)
        {
            IncreaseDifficulty();
        }

        // Limpiar referencias null de obstáculos destruidos
        activeObstacles.RemoveAll(obj => obj == null);
    }

    /// <summary>
    /// Inicia la generación de obstáculos.
    /// </summary>
    private void StartSpawning()
    {
        isSpawning = true;
        Debug.Log("ObstacleSpawner: Generación iniciada");
    }

    /// <summary>
    /// Detiene la generación de obstáculos.
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
        Debug.Log("ObstacleSpawner: Generación detenida");
    }

    /// <summary>
    /// Genera un obstáculo aleatorio.
    /// </summary>
    private void SpawnObstacle()
    {
        // Seleccionar un prefab aleatorio
        GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];

        // Posición de spawn
        Vector3 spawnPosition = new Vector3(nextSpawnX, spawnHeight, 0f);

        // Instanciar obstáculo
        GameObject obstacle = Instantiate(prefab, spawnPosition, Quaternion.identity, transform);

        // Agregar a la lista de activos
        activeObstacles.Add(obstacle);

        // Calcular distancia hasta el próximo obstáculo
        float randomDistance = Random.Range(currentMinDistance, currentMaxDistance);
        nextSpawnX += randomDistance;

        Debug.Log($"Obstáculo generado en X: {spawnPosition.x}, próximo en: {nextSpawnX}");
    }

    /// <summary>
    /// Aumenta la dificultad reduciendo distancia entre obstáculos.
    /// </summary>
    private void IncreaseDifficulty()
    {
        float timeSinceStart = Time.time - gameStartTime;

        // Verificar si es tiempo de aumentar dificultad
        if (timeSinceStart - lastDifficultyIncreaseTime >= difficultyIncreaseInterval)
        {
            lastDifficultyIncreaseTime = timeSinceStart;

            // Reducir distancias (sin bajar del mínimo absoluto)
            currentMinDistance = Mathf.Max(currentMinDistance - distanceDecreaseAmount, absoluteMinDistance);
            currentMaxDistance = Mathf.Max(currentMaxDistance - distanceDecreaseAmount, absoluteMinDistance + 1f);

            Debug.Log($"Dificultad aumentada! Distancia: {currentMinDistance:F1} - {currentMaxDistance:F1}");
        }
    }

    /// <summary>
    /// Limpia todos los obstáculos (útil para restart).
    /// </summary>
    public void ClearAllObstacles()
    {
        foreach (GameObject obstacle in activeObstacles)
        {
            if (obstacle != null)
            {
                Destroy(obstacle);
            }
        }

        activeObstacles.Clear();

        // Resetear posición de spawn
        if (playerTransform != null)
        {
            nextSpawnX = playerTransform.position.x + spawnAheadDistance;
        }

        Debug.Log("Todos los obstáculos eliminados");
    }

    /// <summary>
    /// Reinicia el sistema de dificultad.
    /// </summary>
    public void ResetDifficulty()
    {
        currentMinDistance = minSpawnDistance;
        currentMaxDistance = maxSpawnDistance;
        lastDifficultyIncreaseTime = 0f;
        gameStartTime = Time.time;
    }

    // Visualización de debugging
    private void OnDrawGizmos()
    {
        if (playerTransform == null) return;

        // Dibujar zona de spawn (verde)
        Gizmos.color = Color.green;
        Vector3 spawnPoint = new Vector3(nextSpawnX, spawnHeight, 0f);
        Gizmos.DrawWireSphere(spawnPoint, 0.5f);
        Gizmos.DrawLine(
            new Vector3(nextSpawnX, spawnHeight - 2, 0),
            new Vector3(nextSpawnX, spawnHeight + 2, 0)
        );

        // Dibujar distancia de spawn adelante del jugador (amarillo)
        Gizmos.color = Color.yellow;
        Vector3 spawnTrigger = new Vector3(
            playerTransform.position.x + spawnAheadDistance,
            spawnHeight,
            0f
        );
        Gizmos.DrawWireSphere(spawnTrigger, 0.3f);
    }
}