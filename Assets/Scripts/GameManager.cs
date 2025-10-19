using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float score = 0f;

    public int multiplier = 1;

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

    #endregion

    #region Scene Management

    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
    
    #endregion
}
