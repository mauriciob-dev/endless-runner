using UnityEngine;

/// <summary>
/// Controla el movimiento automático del jugador y el salto táctil.
/// Este es el corazón del endless runner: el personaje corre solo,
/// el jugador solo controla el salto.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f; // Velocidad de carrera (ajustable en Inspector)

    [Header("Salto")]
    [SerializeField] private readonly float jumpForce = 12f; // Fuerza del salto
    [SerializeField] private LayerMask groundLayer; // Qué se considera "suelo"

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck; // Punto desde donde chequeamos si tocamos suelo
    [SerializeField] private readonly float groundCheckRadius = 0.2f; // Radio de detección

    // Referencias privadas
    private Rigidbody2D rb;
    private bool isGrounded; // ¿Está el jugador en el suelo?
    private readonly bool canDoubleJump; // Para implementar doble salto después (opcional)

    // Estado del juego
    private bool isGameActive = true;

    void Start()
    {
        // Obtener referencia al Rigidbody2D del jugador
        rb = GetComponent<Rigidbody2D>();

        // Si no hay un groundCheck asignado, creamos uno automáticamente
        if (groundCheck == null)
        {
            GameObject checkObj = new GameObject("GroundCheck");
            checkObj.transform.SetParent(transform);
            checkObj.transform.localPosition = new Vector3(0, -0.3f, 0); // Ligeramente bajo los pies
            groundCheck = checkObj.transform;
        }
    }

    void Update()
    {
        if (!isGameActive) return; // Si el juego terminó, no hacer nada

        // Chequear si estamos tocando el suelo
        CheckGroundStatus();

        // Detectar input táctil para saltar
        HandleJumpInput();
    }

    void FixedUpdate()
    {
        if (!isGameActive) return;

        // Mantener velocidad horizontal constante mientras está en el suelo
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
        }
    }

    /// <summary>
    /// Detecta si el jugador está tocando el suelo usando Physics2D.OverlapCircle.
    /// Esto es más confiable que usar OnCollisionEnter porque chequea constantemente.
    /// </summary>
    private void CheckGroundStatus()
    {
        // OverlapCircle crea un círculo invisible y detecta si toca algo del groundLayer
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    /// <summary>
    /// Detecta input táctil (o mouse en el editor) y ejecuta el salto.
    /// </summary>
    private void HandleJumpInput()
    {
        // En móvil: detecta cuando se toca la pantalla
        // En editor: detecta click del mouse (para testing)
        bool jumpInputDetected = false;

#if UNITY_EDITOR || UNITY_STANDALONE
        // En el editor o PC, usamos el mouse
        if (Input.GetMouseButtonDown(0))
        {
            jumpInputDetected = true;
        }
#else
        // En móvil, detectamos toques en pantalla
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            jumpInputDetected = true;
        }
#endif

        // Si hay input y estamos en el suelo, saltar
        if (jumpInputDetected && isGrounded)
        {
            Jump();
        }
    }

    /// <summary>
    /// Ejecuta el salto aplicando fuerza vertical al Rigidbody.
    /// </summary>
    private void Jump()
    {
        // Resetear velocidad vertical antes de saltar (para saltos consistentes)
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        // Aplicar fuerza de salto hacia arriba
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // Aquí podrías agregar sonido de salto, animación, partículas, etc.
        Debug.Log("¡Salto!");
    }

    /// <summary>
    /// Detiene el movimiento del jugador (llamado cuando hay Game Over).
    /// </summary>
    public void StopPlayer()
    {
        isGameActive = false;
        rb.linearVelocity = Vector2.zero; // Detener completamente
    }

    /// <summary>
    /// Detecta colisión con obstáculos.
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si choca con un obstáculo, game over
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            GameOver();
        }
    }

    /// <summary>
    /// Maneja el Game Over.
    /// </summary>
    private void GameOver()
    {
        Debug.Log("Player: Game Over detectado");
        StopPlayer();

        // Llamar al GameManager
        GameManager.Instance?.GameOver();
    }

    // Visualización en el editor del área de groundCheck (útil para debugging)
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    /// <summary>
    /// Permite modificar la velocidad desde otros scripts (útil para aumentar dificultad).
    /// </summary>
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
    /// <summary>
    /// Obtiene la velocidad actual del jugador.
    /// </summary>
    public float GetCurrentSpeed()
    {
        return moveSpeed;
    }
}