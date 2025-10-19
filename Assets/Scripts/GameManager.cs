using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Stats")]
    public float score = 0f;
    public int multiplier = 1;
    public int movesLeft = 15;
    public const float MAX_TIME = 90f;
    public float timeLeft = 90f;

    [Header("Game Goals")]
    public const int BLUE_MATCH_GOAL = 3;
    public int blueMatchCount = 0;
    public const int RED_MATCH_GOAL = 3;
    public int redMatchCount = 0;
    public const int GREEN_MATCH_GOAL = 3;
    public int greenMatchCount = 0;
    public const int YELLOW_MATCH_GOAL = 3;
    public int yellowMatchCount = 0;
    public const int PURPLE_MATCH_GOAL = 3;
    public int purpleMatchCount = 0;


    // SINGLETON
    public static GameManager Instance { get; private set; }
    // -----------------

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        score = 0f;
        multiplier = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (timeLeft > 0f)
        {
            timeLeft -= Time.deltaTime;
        }
        else
        {
            timeLeft = 0f;
            // trigger game over
        }
    }

    #region  Game Management

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void AddScore(float points)
    {
        score += points * 100 * multiplier;
    }

    public void IncreaseMultiplier()
    {
        multiplier++;
    }

    public void ResetMultiplier()
    {
        multiplier = 1;
    }

    public void UseMove()
    {
        movesLeft--;
        if (movesLeft < 0)
        {
            movesLeft = 0;
            // trigger game over
        }
    }

    public void AddMatch(TileColor color, int count = 1)
    {
        switch (color)
        {
            case TileColor.Blue:
                blueMatchCount += count;
                break;
            case TileColor.Red:
                redMatchCount += count;
                break;
            case TileColor.Green:
                greenMatchCount += count;
                break;
            case TileColor.Yellow:
                yellowMatchCount += count;
                break;
            case TileColor.Purple:
                purpleMatchCount += count;
                break;
            default:
                Debug.LogWarning("Invalid color for match count.");
                break;
        }
    }

    public bool CheckWinCondition()
    {
        return blueMatchCount >= BLUE_MATCH_GOAL &&
               redMatchCount >= RED_MATCH_GOAL &&
               greenMatchCount >= GREEN_MATCH_GOAL &&
               yellowMatchCount >= YELLOW_MATCH_GOAL &&
               purpleMatchCount >= PURPLE_MATCH_GOAL;
    }

    #endregion

    #region Scene Management

    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
    
    #endregion
}
