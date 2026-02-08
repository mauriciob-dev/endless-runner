using UnityEngine;

/// <summary>
/// Hace que la cámara siga al jugador en un endless runner.
/// Mantiene un offset fijo y solo sigue en el eje X (horizontal).
/// El eje Y se mantiene fijo para evitar que la cámara "salte" cuando el jugador salta.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; // El objeto a seguir (Player)

    [Header("Configuración de Seguimiento")]
    [Tooltip("Offset de la cámara respecto al jugador. X negativo = jugador a la izquierda")]
    [SerializeField] private Vector3 offset = new Vector3(4f, 0f, -10f);

    [Tooltip("¿Seguir solo en X o también en Y?")]
    [SerializeField] private bool followY = false; // false = solo sigue en X

    [Header("Suavizado (Opcional)")]
    [Tooltip("Velocidad de suavizado. 0 = sin suavizado (más responsivo)")]
    [SerializeField] private float smoothSpeed = 0f; // 0 = sin interpolación (mejor para endless runner)

    [Header("Límites (Opcional)")]
    [Tooltip("¿Activar límite mínimo en X? (evita que la cámara retroceda)")]
    [SerializeField] private bool useMinX = true;
    [SerializeField] private float minX = 0f; // La cámara nunca irá más a la izquierda que esto

    // Variables privadas
    private Vector3 velocity = Vector3.zero; // Para SmoothDamp

    void Start()
    {
        // Si no hay target asignado, buscar el Player automáticamente
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("CameraFollow: Target (Player) encontrado automáticamente");
            }
            else
            {
                Debug.LogError("CameraFollow: No se encontró ningún objeto con tag 'Player'!");
            }
        }

        // Posicionar la cámara en la posición inicial correcta
        if (target != null)
        {
            Vector3 initialPos = target.position + offset;
            if (!followY)
            {
                initialPos.y = offset.y; // Mantener Y fijo
            }
            transform.position = initialPos;
        }
    }

    void LateUpdate()
    {
        // LateUpdate se ejecuta DESPUÉS de todos los Update()
        // Esto asegura que el jugador ya se movió antes de actualizar la cámara

        if (target == null) return; // Si no hay target, no hacer nada

        FollowTarget();
    }

    /// <summary>
    /// Calcula y aplica la posición de la cámara siguiendo al target.
    /// </summary>
    private void FollowTarget()
    {
        // Calcular posición deseada
        Vector3 desiredPosition = target.position + offset;

        // Si NO seguimos en Y, mantener la Y del offset original
        if (!followY)
        {
            desiredPosition.y = offset.y;
        }

        // Aplicar límite mínimo en X (evita retroceder)
        if (useMinX)
        {
            desiredPosition.x = Mathf.Max(desiredPosition.x, minX);
        }

        // Aplicar la posición (con o sin suavizado)
        if (smoothSpeed > 0)
        {
            // Con suavizado (interpolación suave)
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref velocity,
                smoothSpeed
            );
        }
        else
        {
            // Sin suavizado (seguimiento exacto) - RECOMENDADO para endless runner
            transform.position = desiredPosition;
        }
    }

    /// <summary>
    /// Permite cambiar el offset en runtime.
    /// Útil para efectos como "zoom out" cuando hay peligro.
    /// </summary>
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }

    /// <summary>
    /// Cambia el target que sigue la cámara.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // Visualización en el editor
    private void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            // Dibuja una línea desde la cámara al target
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target.position);

            // Dibuja la posición deseada de la cámara
            Vector3 desiredPos = target.position + offset;
            if (!followY)
            {
                desiredPos.y = offset.y;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(desiredPos, 0.5f);
        }
    }
}