using UnityEngine;

/// <summary>
/// Componente para obstáculos individuales.
/// Se autodestruye cuando queda fuera de la cámara.
/// </summary>
public class Obstacle : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Puntos que otorga al ser esquivado exitosamente")]
    [SerializeField] private int scoreValue = 10;

    [Tooltip("Velocidad de movimiento (si es 0, usa la del spawner)")]
    [SerializeField] private float moveSpeed = 0f;

    private bool hasBeenPassed = false; // ¿El jugador ya pasó este obstáculo?
    private Transform playerTransform;
    private Camera mainCamera;

    void Start()
    {
        // Buscar referencias
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        mainCamera = Camera.main;
    }

    void Update()
    {
        // Moverse hacia la izquierda (si tiene velocidad propia)
        if (moveSpeed > 0)
        {
            transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
        }

        // Verificar si el jugador pasó el obstáculo (dar puntos)
        if (!hasBeenPassed && playerTransform != null)
        {
            if (playerTransform.position.x > transform.position.x + 1f) // +1 para dar margen
            {
                hasBeenPassed = true;
                OnObstaclePassed();
            }
        }

        // Autodestruirse si está muy fuera de cámara
        CheckIfOffScreen();
    }

    /// <summary>
    /// Llamado cuando el jugador esquiva exitosamente el obstáculo.
    /// </summary>
    private void OnObstaclePassed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        Debug.Log($"Obstáculo esquivado! +{scoreValue} puntos");
    }

    /// <summary>
    /// Destruye el obstáculo si está fuera de cámara.
    /// </summary>
    private void CheckIfOffScreen()
    {
        if (mainCamera == null) return;

        // Calcular límite izquierdo de la cámara
        float cameraLeft = mainCamera.transform.position.x - mainCamera.orthographicSize * mainCamera.aspect - 2f;

        // Si el obstáculo está a la izquierda del límite, destruirlo
        if (transform.position.x < cameraLeft)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Método público para ser llamado por el spawner si usa pooling.
    /// </summary>
    public void ResetObstacle()
    {
        hasBeenPassed = false;
    }
}