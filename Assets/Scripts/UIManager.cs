using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject titlePanel;

    void Start()
    {
        // Start with the title screen active and the game paused
        if (titlePanel != null)
        {
            titlePanel.SetActive(true);
            Time.timeScale = 0;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void StartGame()
    {
        if (titlePanel != null)
        {
            titlePanel.SetActive(false);
            Time.timeScale = 1;
        }
    }

    public void TogglePause()
    {
        if (pauseMenu != null)
        {
            bool isPaused = !pauseMenu.activeSelf;
            pauseMenu.SetActive(isPaused);
            Time.timeScale = isPaused ? 0 : 1;
        }
    }

    public void RestartGame()
    {
        // We start a coroutine to handle the delay
        StartCoroutine(RestartGameRoutine());
    }

    private IEnumerator RestartGameRoutine()
    {
        // Play the click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick();
        }

        // Wait for a short duration, ignoring the paused time scale
        yield return new WaitForSecondsRealtime(0.2f);

        // Now, unpause and reload the scene
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
