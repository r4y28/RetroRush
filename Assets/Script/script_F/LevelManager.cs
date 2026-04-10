using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    [Header("Game State")]
    public bool isGameOver = false;
    public bool isGamePaused = false;

    [Header("UI")]
    public GameObject gameOverUI;
    public GameObject pauseUI;

 

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // -----------------------------
    // 🎮 GAME FLOW
    // -----------------------------

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;

        if (gameOverUI != null)
            gameOverUI.SetActive(true);
    }

    public void WinLevel()
    {
        Time.timeScale = 0f;
        Debug.Log("Level Complete!");
        // You can add win UI here
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.Log("No more levels!");
        }
    }

    public void LoadLevel(int index)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(index);
    }

    // -----------------------------
    // ⏸️ PAUSE SYSTEM
    // -----------------------------

    public void TogglePause()
    {
        if (isGamePaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f;

        if (pauseUI != null)
            pauseUI.SetActive(true);
    }

    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;

        if (pauseUI != null)
            pauseUI.SetActive(false);
    }

    // -----------------------------
    // 🚪 QUIT
    // -----------------------------

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}