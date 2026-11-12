using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References (Legacy UI Text)")]
    [Tooltip("Komponen UI Text untuk menampilkan skor aktif saat bermain")]
    [SerializeField] private Text scoreText;

    [Tooltip("Komponen UI Text untuk menampilkan skor akhir pada panel Game Over")]
    [SerializeField] private Text finalScoreText;

    [Header("Panels & Buttons")]
    [Tooltip("Panel Game Over yang muncul saat burung kalah")]
    [SerializeField] private GameObject gameOverPanel;

    [Tooltip("Tombol restart pada panel Game Over")]
    [SerializeField] private Button restartButton;

    private int currentScore = 0;
    private bool isGameOver = false;

    public int CurrentScore => currentScore;
    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Pastikan game berjalan normal saat awal
        Time.timeScale = 1f;
        isGameOver = false;
        currentScore = 0;
        UpdateScoreDisplay();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    public void AddScore()
    {
        if (isGameOver) return;

        currentScore++;
        UpdateScoreDisplay();
        Debug.Log("[GameManager] Skor bertambah: " + currentScore);
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f;

        if (finalScoreText != null)
        {
            finalScoreText.text = "Score: " + currentScore;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Debug.Log("[GameManager] GAME OVER! Skor Akhir: " + currentScore);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
