using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Genera suelo infinito usando Object Pooling.
/// Recicla tiles de suelo para optimizar rendimiento.
/// </summary>
public class GroundSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [Tooltip("El prefab de tile de suelo a instanciar")]
    [SerializeField] private GameObject groundTilePrefab;

    [Header("Configuración de Generación")]
    [Tooltip("Número de tiles que se generan inicialmente")]
    [SerializeField] private int numberOfTiles = 6;

    [Tooltip("Ancho de cada tile de suelo (debe coincidir con el prefab)")]
    [SerializeField] private float tileWidth = 5f;

    [Tooltip("Posición Z de los tiles (debe ser 0 para 2D)")]
    [SerializeField] private float zPosition = 0f;

    [Tooltip("Altura Y del suelo")]
    [SerializeField] private float groundHeight = -3f;

    [Header("Referencias")]
    [Tooltip("El transform del jugador para saber cuándo generar más tiles")]
    [SerializeField] private Transform playerTransform;

    [Header("Optimización")]
    [Tooltip("Distancia adelante del jugador donde se generan tiles")]
    [SerializeField] private float spawnDistance = 20f;

    [Tooltip("Distancia atrás del jugador donde se destruyen/reciclan tiles")]
    [SerializeField] private float despawnDistance = 10f;

    // Variables privadas
    private List<GameObject> activeTiles = new List<GameObject>(); // Tiles activos en escena
    private float nextSpawnX; // Posición X donde se generará el próximo tile

    // Pool de tiles (para reutilizar)
    private Queue<GameObject> tilePool = new Queue<GameObject>();

    void Start()
    {
        // Buscar el jugador automáticamente si no está asignado
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log("GroundSpawner: Player encontrado automáticamente");
            }
            else
            {
                Debug.LogError("GroundSpawner: No se encontró el Player! Asigna el tag 'Player'");
                return;
            }
        }

        // Verificar que hay prefab asignado
        if (groundTilePrefab == null)
        {
            Debug.LogError("GroundSpawner: No hay prefab asignado! Arrastra el prefab GroundTile al script.");
            return;
        }

        // Calcular posición inicial (un poco atrás del jugador)
        nextSpawnX = playerTransform.position.x - tileWidth;

        // Generar tiles iniciales
        for (int i = 0; i < numberOfTiles; i++)
        {
            SpawnTile();
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Generar tiles adelante del jugador
        if (playerTransform.position.x + spawnDistance > nextSpawnX)
        {
            SpawnTile();
        }

        // Reciclar tiles que quedaron muy atrás
        RecycleTiles();
    }

    /// <summary>
    /// Genera (o recicla) un nuevo tile de suelo.
    /// </summary>
    private void SpawnTile()
    {
        GameObject tile;

        // Intentar reutilizar un tile del pool
        if (tilePool.Count > 0)
        {
            tile = tilePool.Dequeue();
            tile.SetActive(true);
        }
        else
        {
            // Si no hay tiles en el pool, crear uno nuevo
            tile = Instantiate(groundTilePrefab, transform); // transform = hacer hijo de este objeto
        }

        // Posicionar el tile
        tile.transform.position = new Vector3(nextSpawnX, groundHeight, zPosition);

        // Agregar a la lista de tiles activos
        activeTiles.Add(tile);

        // Calcular dónde irá el próximo tile
        nextSpawnX += tileWidth;
    }

    /// <summary>
    /// Recicla (desactiva) tiles que quedaron muy atrás del jugador.
    /// </summary>
    private void RecycleTiles()
    {
        // Revisar los tiles activos (normalmente el primero es el más viejo)
        if (activeTiles.Count > 0)
        {
            GameObject firstTile = activeTiles[0];

            // Si el tile está muy atrás del jugador, reciclarlo
            float distanceFromPlayer = playerTransform.position.x - firstTile.transform.position.x;

            if (distanceFromPlayer > despawnDistance)
            {
                // Desactivar el tile y agregarlo al pool
                firstTile.SetActive(false);
                tilePool.Enqueue(firstTile);

                // Quitarlo de la lista de activos
                activeTiles.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// Limpia todos los tiles (útil para reiniciar el juego).
    /// </summary>
    public void ClearAllTiles()
    {
        // Desactivar todos los tiles activos
        foreach (GameObject tile in activeTiles)
        {
            tile.SetActive(false);
            tilePool.Enqueue(tile);
        }

        activeTiles.Clear();

        // Resetear la posición de spawn
        if (playerTransform != null)
        {
            nextSpawnX = playerTransform.position.x - tileWidth;
        }
    }

    /// <summary>
    /// Cambia la altura del suelo en runtime.
    /// </summary>
    public void SetGroundHeight(float newHeight)
    {
        groundHeight = newHeight;

        // Actualizar tiles existentes
        foreach (GameObject tile in activeTiles)
        {
            Vector3 pos = tile.transform.position;
            pos.y = groundHeight;
            tile.transform.position = pos;
        }
    }

    // Visualización de debug en el editor
    private void OnDrawGizmos()
    {
        if (playerTransform == null) return;

        // Dibujar zona de spawn (verde)
        Gizmos.color = Color.green;
        Vector3 spawnPoint = new Vector3(
            playerTransform.position.x + spawnDistance,
            groundHeight,
            0
        );
        Gizmos.DrawWireSphere(spawnPoint, 0.5f);

        // Dibujar zona de despawn (rojo)
        Gizmos.color = Color.red;
        Vector3 despawnPoint = new Vector3(
            playerTransform.position.x - despawnDistance,
            groundHeight,
            0
        );
        Gizmos.DrawWireSphere(despawnPoint, 0.5f);

        // Dibujar línea de referencia
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            new Vector3(playerTransform.position.x - despawnDistance, groundHeight, 0),
            new Vector3(playerTransform.position.x + spawnDistance, groundHeight, 0)
        );
    }
}