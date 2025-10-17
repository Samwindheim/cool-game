using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

// This class manages UI state and navigation, such as the title screen and pause menu.
public class UIManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject titlePanel;
    
    // This flag tracks if the game is in an "active" playable state.
    // It's used to prevent pausing from the title or game over screens.
    private bool isGameActive = false;

    void Start()
    {
        // When the scene loads, show the title screen and pause the game.
        if (titlePanel != null)
        {
            titlePanel.SetActive(true);
        }
        Time.timeScale = 0;
        isGameActive = false;
    }

    void Update()
    {
        // Only listen for the pause key if the game is currently active.
        if (isGameActive && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // Allows the GameManager to control the active state of the UI.
    public void SetGameActive(bool isActive)
    {
        isGameActive = isActive;
    }

    // Called by the "Start Game" button on the title screen.
    public void StartGame()
    {
        if (titlePanel != null)
        {
            titlePanel.SetActive(false);
            Time.timeScale = 1; // Unpause the game.
            SetGameActive(true);

            // Start the background music now that the user has interacted with the page.
            // This is required for audio to work in most web browsers.
            if (AudioManager.Instance != null && AudioManager.Instance.bgMusic != null)
            {
                if (!AudioManager.Instance.bgMusic.isPlaying)
                {
                    AudioManager.Instance.bgMusic.Play();
                }
            }
        }
    }

    // Toggles the pause state of the game.
    public void TogglePause()
    {
        if (pauseMenu != null)
        {
            bool isPaused = !pauseMenu.activeSelf;
            pauseMenu.SetActive(isPaused);
            
            // Setting Time.timeScale to 0 pauses all physics and animations.
            // Setting it to 1 resumes normal time.
            Time.timeScale = isPaused ? 0 : 1;

            // When unpausing, we clear the EventSystem's selected object.
            // This prevents buttons from getting visually "stuck" in their pressed state.
            if (!isPaused)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    // This is the public method called by the restart buttons.
    public void RestartGame()
    {
        // We start a coroutine so we can add a delay, allowing the click sound to play
        // before the scene gets reloaded and destroys the AudioManager.
        StartCoroutine(RestartGameRoutine());
    }

    // This coroutine handles the actual restart sequence.
    private IEnumerator RestartGameRoutine()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick();
        }

        // We must use WaitForSecondsRealtime because Time.timeScale might be 0,
        // which would cause a normal WaitForSeconds to wait forever.
        yield return new WaitForSecondsRealtime(0.2f);

        // Reset time scale to normal before reloading the scene.
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
