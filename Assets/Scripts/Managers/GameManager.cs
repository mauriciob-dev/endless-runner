using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Para TextMeshPro

/// <summary>
/// Controla el estado global del juego, puntuación y flujo de juego.
/// Implementa el patrón Singleton para acceso global.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton - Solo puede existir una instancia
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    [Header("Player Reference")]
    [SerializeField] private PlayerController playerController;

    [Header("Game Settings")]
    [Tooltip("Puntos ganados por segundo de juego")]
    [SerializeField] private float scorePerSecond = 10f;

    [Tooltip("¿Aumentar velocidad del jugador con el tiempo?")]
    [SerializeField] private bool increaseSpeedOverTime = true;

    [Tooltip("Velocidad máxima del jugador")]
    [SerializeField] private float maxSpeed = 15f;

    [Tooltip("Cada cuántos segundos aumenta la velocidad")]
    [SerializeField] private float speedIncreaseInterval = 10f;

    [Tooltip("Cuánto aumenta la velocidad cada intervalo")]
    [SerializeField] private float speedIncreaseAmount = 0.5f;

    // Variables de estado
    private int currentScore = 0;
    private float gameTime = 0f;
    private bool isGameActive = false;
    private float lastSpeedIncreaseTime = 0f;

    // Estados del juego
    public enum GameState
    {
        Playing,
        Paused,
        GameOver
    }

    private GameState currentState = GameState.Playing;

    void Awake()
    {
        // Implementar Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Buscar referencias automáticamente si no están asignadas
        if (scoreText == null)
        {
            scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
        }

        if (gameOverPanel == null)
        {
            gameOverPanel = GameObject.Find("GameOverPanel");
        }

        if (finalScoreText == null)
        {
            finalScoreText = GameObject.Find("FinalScoreText")?.GetComponent<TextMeshProUGUI>();
        }

        if (playerController == null)
        {
            playerController = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
        }

        // Verificar referencias críticas
        if (playerController == null)
        {
            Debug.LogError("GameManager: No se encontró PlayerController!");
        }

        // Iniciar el juego
        StartGame();
    }

    void Update()
    {
        if (!isGameActive || currentState != GameState.Playing) return;

        // Actualizar tiempo de juego
        gameTime += Time.deltaTime;

        // Calcular y actualizar puntuación por tiempo
        UpdateScore();

        // Aumentar velocidad progresivamente
        if (increaseSpeedOverTime)
        {
            IncreaseSpeedOverTime();
        }
    }

    /// <summary>
    /// Inicia el juego (primera vez o después de restart).
    /// </summary>
    private void StartGame()
    {
        isGameActive = true;
        currentState = GameState.Playing;
        currentScore = 0;
        gameTime = 0f;
        lastSpeedIncreaseTime = 0f;

        // Ocultar panel de Game Over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Actualizar UI
        UpdateScoreUI();

        // Reactivar el jugador si estaba detenido
        if (playerController != null)
        {
            // El PlayerController maneja su propio estado
        }

        Debug.Log("Game Started!");
    }

    /// <summary>
    /// Actualiza la puntuación basada en el tiempo de juego.
    /// </summary>
    private void UpdateScore()
    {
        // Ganar puntos por tiempo (distancia recorrida implícita)
        currentScore = Mathf.FloorToInt(gameTime * scorePerSecond);
        UpdateScoreUI();
    }

    /// <summary>
    /// Aumenta la velocidad del jugador progresivamente.
    /// </summary>
    private void IncreaseSpeedOverTime()
    {
        if (playerController == null) return;

        // Verificar si es tiempo de aumentar velocidad
        if (gameTime - lastSpeedIncreaseTime >= speedIncreaseInterval)
        {
            lastSpeedIncreaseTime = gameTime;

            // Aumentar velocidad (sin exceder el máximo)
            float currentSpeed = playerController.GetCurrentSpeed();
            float newSpeed = Mathf.Min(currentSpeed + speedIncreaseAmount, maxSpeed);
            playerController.SetMoveSpeed(newSpeed);

            Debug.Log($"Speed increased to: {newSpeed:F1}");
        }
    }

    /// <summary>
    /// Actualiza el texto de puntuación en pantalla.
    /// </summary>
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }

    /// <summary>
    /// Agrega puntos manualmente (útil para coleccionables).
    /// </summary>
    public void AddScore(int points)
    {
        if (!isGameActive) return;

        currentScore += points;
        UpdateScoreUI();
    }

    /// <summary>
    /// Llamar cuando el jugador muere.
    /// </summary>
    public void GameOver()
    {
        if (currentState == GameState.GameOver) return; // Evitar llamadas múltiples

        isGameActive = false;
        currentState = GameState.GameOver;

        Debug.Log($"Game Over! Final Score: {currentScore}");

        // Detener al jugador
        if (playerController != null)
        {
            playerController.StopPlayer();
        }

        // Mostrar panel de Game Over
        ShowGameOverScreen();
    }

    /// <summary>
    /// Muestra la pantalla de Game Over con puntuación final.
    /// </summary>
    private void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {currentScore}";
        }
    }

    /// <summary>
    /// Reinicia el juego (recarga la escena).
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("Restarting game...");

        // Recargar la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Pausa el juego.
    /// </summary>
    public void PauseGame()
    {
        if (currentState == GameState.GameOver) return;

        currentState = GameState.Paused;
        Time.timeScale = 0f; // Detiene el tiempo del juego

        Debug.Log("Game Paused");
    }

    /// <summary>
    /// Reanuda el juego.
    /// </summary>
    public void ResumeGame()
    {
        if (currentState == GameState.GameOver) return;

        currentState = GameState.Playing;
        Time.timeScale = 1f; // Restaura el tiempo normal

        Debug.Log("Game Resumed");
    }

    // Getters públicos
    public int GetCurrentScore() => currentScore;
    public float GetGameTime() => gameTime;
    public bool IsGameActive() => isGameActive;
    public GameState GetCurrentState() => currentState;
}