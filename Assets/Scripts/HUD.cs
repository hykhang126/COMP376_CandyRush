using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI winScoreText;
    public TextMeshProUGUI multiplierText;
    public Slider timeSlider;
    public TextMeshProUGUI movesLeftText;

    public TextMeshProUGUI blueMatchCountText;
    public TextMeshProUGUI redMatchCountText;
    public TextMeshProUGUI greenMatchCountText;
    public TextMeshProUGUI yellowMatchCountText;
    public TextMeshProUGUI purpleMatchCountText;

    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;
    public GameObject gameWinPanel;
    public GameObject[] starsPrefabs;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.OnGamePause.AddListener(HandleGamePause);
        GameManager.Instance.OnGameResume.AddListener(HandleGameResume);
        GameManager.Instance.OnGameOver.AddListener(HandleGameOver);
        GameManager.Instance.OnGameWin.AddListener(HandleGameWin);

        UpdateScoreText();
        UpdateMultiplierText();
        UpdateTimeSlider();
        UpdateMovesLeftText();
        UpdateMatchCountTexts();

        // Hide all panels at start
        pauseMenuPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        gameWinPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateScoreText();
        UpdateMultiplierText();
        UpdateTimeSlider();
        UpdateMovesLeftText();
        UpdateMatchCountTexts();
    }

    #region UI Update Methods

    private void UpdateScoreText()
    {
        scoreText.text = $"{GameManager.Instance.score}";
        winScoreText.text = $"{GameManager.Instance.score}";
    }

    private void UpdateMultiplierText()
    {
        multiplierText.text = $"x{GameManager.Instance.multiplier}";
    }

    private void UpdateTimeSlider()
    {
        timeSlider.value = GameManager.Instance.timeLeft / GameManager.MAX_TIME;
    }

    private void UpdateMovesLeftText()
    {
        movesLeftText.text = $"{GameManager.Instance.movesLeft}";
    }

    private void UpdateMatchCountTexts()
    {
        blueMatchCountText.text = $"{GameManager.Instance.blueMatchCount}/{GameManager.BLUE_MATCH_GOAL}";
        redMatchCountText.text = $"{GameManager.Instance.redMatchCount}/{GameManager.RED_MATCH_GOAL}";
        greenMatchCountText.text = $"{GameManager.Instance.greenMatchCount}/{GameManager.GREEN_MATCH_GOAL}";
        yellowMatchCountText.text = $"{GameManager.Instance.yellowMatchCount}/{GameManager.YELLOW_MATCH_GOAL}";
        purpleMatchCountText.text = $"{GameManager.Instance.purpleMatchCount}/{GameManager.PURPLE_MATCH_GOAL}";
    }

    #endregion

    #region Events Handlers

    public void HandleGamePause()
    {
        // Show Pause Menu UI
        pauseMenuPanel.SetActive(true);
    }

    public void HandleGameResume()
    {
        // Hide Pause Menu UI
        pauseMenuPanel.SetActive(false);
    }

    public void HandleGameOver()
    {
        // Show Game Over UI
        gameOverPanel.SetActive(true);
    }

    public void HandleGameWin()
    {
        // Show Game Win UI
        gameWinPanel.SetActive(true);

        // Spawn Stars based on score
        int starsEarned = GameManager.Instance.score >= 10000 ? 3 :
                          GameManager.Instance.score >= 5000 ? 2 : 1;

        for (int i = 0; i < starsEarned; i++)
        {
            starsPrefabs[i].SetActive(true);
        }
    }

    public void OnClickResumeButton()
    {
        GameManager.Instance.OnGameResume.Invoke();
    }
    
    public void OnClickPauseButton()
    {
        if (GameManager.Instance.currentGameState == GameState.Playing)
        {
            GameManager.Instance.OnGamePause.Invoke();
        }
    }

    #endregion
}
