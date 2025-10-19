using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI multiplierText;
    public Slider timeSlider;
    public TextMeshProUGUI movesLeftText;

    public TextMeshProUGUI blueMatchCountText;
    public TextMeshProUGUI redMatchCountText;
    public TextMeshProUGUI greenMatchCountText;
    public TextMeshProUGUI yellowMatchCountText;
    public TextMeshProUGUI purpleMatchCountText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateScoreText();
        UpdateMultiplierText();
        UpdateTimeSlider();
        UpdateMovesLeftText();
        UpdateMatchCountTexts();
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

    private void UpdateScoreText()
    {
        scoreText.text = $"{GameManager.Instance.score}";
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
}
